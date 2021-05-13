---------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------
-- Создание объектов, версия 16
-- Ускорение процедуры
---------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------

CREATE OR REPLACE PROCEDURE opd_cshd.pai_process_services(
	p_progress character varying)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
	maxIteration int;
	currentIteration int;
BEGIN

	-- Удаляем ненужные записи
	DELETE FROM opd_cshd.ai_ensi_key_iteration k WHERE progress = p_progress;

	-- Формируем итерации для дальнейшей обработки сервисов
	INSERT INTO opd_cshd.ai_ensi_key_iteration (progress, iteration, message_marker, ensi_key, ensi_servicelink, is_prp, event_date_time, operation_pid, operation_type)
	SELECT p_progress, ROW_NUMBER() OVER(PARTITION BY k.ensi_key ORDER BY CASE WHEN p.operation_type = 'CREATE' THEN 0 ELSE 1 END, p.event_date_time),
			k.message_marker, k.ensi_key, k.ensi_servicelink, CASE WHEN p.account_type = 'PRP' THEN 1 ELSE 0 END,
			p.event_date_time, p.operation_pid, p.operation_type
		FROM opd_cshd.ai_ensi_key k
		INNER JOIN opd_cshd.a_person p ON k.message_marker = p.message_marker
		WHERE k.progress = p_progress AND p.progress = p_progress AND
			p.operation_type IN ('CREATE', 'UPDATE') AND
			NOT EXISTS (SELECT * FROM opd_cshd.ai_excluded s WHERE s.progress = p_progress AND s.message_marker = k.message_marker);
	RAISE NOTICE 'ai_ensi_key_iteration %', clock_timestamp();

	-- Удаляем ненужные записи
	DELETE FROM opd_cshd.ai_service_iteration k WHERE progress = p_progress;

	ANALYZE opd_cshd.ai_ensi_key_iteration;
	ANALYZE opd_cshd.a_person;
	ANALYZE opd_cshd.a_service;
	RAISE NOTICE 'analyze %', clock_timestamp();

	-- Организуем итеративную обработку сервисов. В итерации только одна запись о ФЛ
	SELECT MAX(iteration) into maxIteration
		FROM opd_cshd.ai_ensi_key_iteration
		WHERE progress = p_progress;

	IF maxIteration IS NULL THEN
		RETURN;
	END IF;

	-- Для всех сообщений с account_type = 'PRP' создаём запись о сервисе
	INSERT INTO opd_cshd.a_service (progress, message_marker, service_name, status_service, activation_date_time, event_date_time)
	SELECT p_progress, i.message_marker, 'Портал', 'Сервис подключен', null::timestamp, p.event_date_time
		FROM opd_cshd.ai_ensi_key_iteration i
		INNER JOIN opd_cshd.a_person p ON i.message_marker = p.message_marker
		WHERE i.progress = p_progress AND p.progress = p_progress AND i.iteration = 1 AND i.is_prp = 1;
		RAISE NOTICE 'prp %', clock_timestamp();

	-- На первой итерации присутствуют все ФЛ, поэтому по первой итерации получаем исходное состояние из ЕНСИ
	INSERT INTO opd_cshd.ai_service_iteration (progress, message_marker, service_name, is_prp,
		prev_service_key, prev_service_status, prev_service_activationdt, prev_service_source,
		curr_pk, curr_service_status, curr_service_activationdt, curr_service_source,
		event_date_time, operation_type, operation_pid)
	SELECT p_progress, COALESCE(curr.message_marker, prev.message_marker),
			COALESCE(curr.service_name, prev.kindname), COALESCE(curr.is_prp, prev.is_prp),
			prev.code, prev.status, prev.changedate, prev.source,
			curr.pk, curr.status_service, curr.activation_date_time, curr.source_service,
			COALESCE(curr.event_date_time, prev.event_date_time), COALESCE(curr.operation_type, prev.operation_type),
			COALESCE(curr.operation_pid, prev.operation_pid)
		FROM
		(
			SELECT i.message_marker, v.kindname, v.code, v.status, v.changedate, v.source,
					i.event_date_time, i.operation_type, i.operation_pid,
					CASE WHEN i.is_prp = 1 AND v.kindname = 'Портал' THEN 1 ELSE 0 END is_prp
				FROM opd_cshd.ai_ensi_key_iteration i
				INNER JOIN MULTIPLELINK ml ON ml.linkguid = i.ensi_servicelink
				INNER JOIN opd_cshd.v_service_act v ON v.objectid = ml.ancobjectid AND v.kindname IS NOT NULL
				WHERE i.progress = p_progress AND i.iteration = 1
		) prev
		FULL OUTER JOIN
		(
			SELECT s.message_marker, s.service_name, s.pk, s.status_service, s.activation_date_time, s.source_service,
					i.event_date_time, i.operation_type, i.operation_pid,
					CASE WHEN i.is_prp = 1 AND s.service_name = 'Портал' THEN 1 ELSE 0 END is_prp
				FROM opd_cshd.ai_ensi_key_iteration i
				INNER JOIN opd_cshd.a_service s ON s.message_marker = i.message_marker
				WHERE i.progress = p_progress AND s.progress = p_progress AND i.iteration = 1
		) curr ON prev.message_marker = curr.message_marker AND prev.kindname = curr.service_name;

	FOR currentIteration IN 1..maxIteration LOOP
		RAISE NOTICE 'Итерация %, %', currentIteration, clock_timestamp();
		ANALYZE opd_cshd.ai_service_iteration;
		--RAISE NOTICE 'analyze %, %', currentIteration, clock_timestamp();

		---------------------------------------------------------------------------------------------------------
		-- 9.2 Обработка сервисов типа "Портал" из сообщений с account_type = 'PRP'
		---------------------------------------------------------------------------------------------------------

		-- Добавляем новые записи для operation_type = CREATE
		UPDATE opd_cshd.a_service s SET
				type_service = opd_cshd.fai_get_servicetype(k.service_name),
				ensi_code = k.prev_service_key,
				activation_date_time = CASE
					WHEN k.mode = 1 THEN event_date_time
					WHEN k.mode = 2 THEN NULL
					ELSE k.prev_service_activationdt END,
				source_service = CASE
					WHEN k.mode = 1 THEN opd_cshd.fai_get_source(k.operation_pid)
					WHEN k.mode = 2 THEN NULL
					ELSE k.prev_service_source END
			FROM
			(
				SELECT k.curr_pk, k.progress, k.message_marker, k.service_name, k.prev_service_key,
						k.prev_service_activationdt, k.prev_service_source, k.operation_pid,
						CASE
							WHEN k.operation_type = 'CREATE'
								AND
								(
									-- В ЕНСИ есть подключенный сервис
									k.prev_service_status = 'Сервис подключен'
									OR
									-- В ЕНСИ есть отключенный сервис и дата отключения <= event_date_time
									k.prev_service_status = 'Сервис отключен' AND k.prev_service_activationdt <= k.event_date_time
									OR
									-- В ЕНСИ нет записи
									k.prev_service_key IS NULL
								)
								THEN 1
							WHEN k.operation_type = 'UPDATE'
								AND
								(
									-- В ЕНСИ есть отключенный сервис и дата отключения <= event_date_time
									k.prev_service_status = 'Сервис отключен' AND k.prev_service_activationdt <= k.event_date_time
									OR
									-- В ЕНСИ нет записи
									k.prev_service_key IS NULL
								)
								THEN 2
							-- Записи, которые просто нужно скопировать из предыдущего сообщения
							ELSE 3 END as mode
					FROM opd_cshd.ai_service_iteration k
					WHERE k.progress = p_progress AND k.is_prp = 1
			) k
			WHERE s.pk = k.curr_pk;

		---------------------------------------------------------------------------------------------------------
		-- 9.1 Обработка стандартных записей в a_service
		---------------------------------------------------------------------------------------------------------

		-- Здесь просто копируем данные из
		UPDATE opd_cshd.a_service s SET
				ensi_code = k.prev_service_key,
				status_service = CASE
					WHEN k.mode = 1 THEN k.prev_service_status
					ELSE 'Сервис подключен'
					END,
				source_service = CASE
					WHEN k.mode = 1 THEN k.prev_service_source
					WHEN k.mode = 3 THEN NULL
					ELSE opd_cshd.fai_get_source(k.operation_pid)
					END,
				activation_date_time = CASE
					WHEN k.mode = 1 THEN k.prev_service_activationdt
					ELSE activation_date_time
					END
			FROM
			(
				SELECT k.curr_pk, k.prev_service_key, k.prev_service_status, k.prev_service_source, k.prev_service_activationdt, k.operation_pid,
						CASE
							-- Запись в a_service есть, запись в ЕНСИ найдена, статус "Отключен", дата дезактивации сервиса в ЕНСИ больше даты активации в сообщении. 3.1.1.1.1.1.1
							WHEN
								(
									k.prev_service_status = 'Сервис отключен' AND k.prev_service_activationdt > k.curr_service_activationdt
									OR
									k.prev_service_status = 'Сервис подключен' AND k.prev_service_activationdt <= k.curr_service_activationdt
								)
								THEN 1
							-- Запись в a_service есть, запись в ЕНСИ найдена, статус "Подключен", 3.1.1.1.1.2
							WHEN k.prev_service_status = 'Сервис подключен' AND k.prev_service_activationdt > k.curr_service_activationdt
								THEN 2
							WHEN k.prev_service_status = 'Сервис отключен' AND k.prev_service_activationdt <= k.curr_service_activationdt
								THEN 3
							-- Запись a_service есть, в ЕНСИ нет
							ELSE 4 END as mode
					FROM opd_cshd.ai_service_iteration k
					WHERE k.progress = p_progress AND k.is_prp = 0 AND k.curr_pk IS NOT NULL
			) k
			WHERE k.curr_pk = s.pk;

		-- Добавить запись об отключении сервиса. 3.1.1.2
		INSERT INTO opd_cshd.a_service (progress, message_marker, service_name, type_service, ensi_code, status_service,
				source_service, event_date_time, activation_date_time)
		SELECT p_progress, k.message_marker, k.service_name, opd_cshd.fai_get_servicetype(k.service_name),
				k.prev_service_key, 'Сервис отключен', opd_cshd.fai_get_source(k.operation_pid), k.event_date_time,
				CASE WHEN k.prev_service_activationdt <= k.event_date_time AND k.prev_service_status = 'Сервис отключен' THEN k.prev_service_activationdt ELSE k.event_date_time END
			FROM opd_cshd.ai_service_iteration k
			WHERE k.progress = p_progress AND k.is_prp = 0 AND
				-- Записи в a_service нет, в ЕНСИ есть подключенный сервис (или отключенный, не важно)
				k.curr_pk IS NULL AND k.prev_service_key IS NOT NULL;
		--RAISE NOTICE 'update1 %, %', currentIteration, clock_timestamp();

		IF currentIteration > 1 THEN
			-- Пробуем установить код ЕНСИ по данным предыдущих итераций
			UPDATE opd_cshd.a_service s SET
					ensi_code = iprev.ensi_code
				FROM opd_cshd.ai_ensi_key_iteration i
				INNER JOIN
				(
					SELECT i.ensi_key service_key, s.service_name, MIN(s.ensi_code) ensi_code
						FROM opd_cshd.ai_ensi_key_iteration i
						INNER JOIN opd_cshd.a_service s ON i.message_marker = s.message_marker
						WHERE s.progress = p_progress AND s.ensi_code IS NULL AND i.progress = p_progress AND i.iteration < currentIteration
						GROUP BY i.ensi_key, s.service_name
				) iprev ON i.ensi_key = iprev.service_key
				WHERE s.progress = p_progress AND s.ensi_code IS NULL AND
					i.progress = p_progress AND i.iteration = currentIteration AND
					i.message_marker = s.message_marker AND
					s.service_name = iprev.service_name;
		END IF;
		--RAISE NOTICE 'update2 %, %', currentIteration, clock_timestamp();

		-- Устанавливаем автоинкрементные коды для тех, кому они всё ещё не назначены
		CALL opd_cshd.pai_set_service_autoinccodes(p_progress, currentIteration);
		--RAISE NOTICE 'autoinc %, %', currentIteration, clock_timestamp();

		-- Удаляем ненужные записи
		DELETE FROM opd_cshd.ai_service_iteration k WHERE progress = p_progress;

		IF currentIteration < maxIteration THEN
			-- Для всех сообщений с account_type = 'PRP' создаём запись о сервисе
			INSERT INTO opd_cshd.a_service (progress, message_marker, service_name, status_service, activation_date_time, event_date_time)
			SELECT p_progress, i.message_marker, 'Портал', 'Сервис подключен', null::timestamp, i.event_date_time
				FROM opd_cshd.ai_ensi_key_iteration i
				WHERE i.progress = p_progress AND i.iteration = currentIteration + 1 AND i.is_prp = 1;

			-- На первой итерации присутствуют все ФЛ, поэтому по первой итерации получаем исходное состояние из ЕНСИ
			INSERT INTO opd_cshd.ai_service_iteration (progress, message_marker, service_name, is_prp,
				prev_service_key, prev_service_status, prev_service_activationdt, prev_service_source,
				curr_pk, curr_service_status, curr_service_activationdt, curr_service_source,
				event_date_time, operation_type, operation_pid)
			SELECT p_progress, COALESCE(curr.message_marker, prev.message_marker),
					COALESCE(curr.service_name, prev.service_name), COALESCE(curr.is_prp, prev.is_prp),
					prev.service_code, prev.status_service, prev.activation_date_time, prev.source_service,
					curr.pk, curr.status_service, curr.activation_date_time, curr.source_service,
					COALESCE(curr.event_date_time, prev.event_date_time), COALESCE(curr.operation_type, prev.operation_type),
					COALESCE(curr.operation_pid, prev.operation_pid)
				FROM
				(
					SELECT icurr.message_marker, s.service_name, s.ensi_code service_code, s.status_service, s.activation_date_time,
							s.source_service, i.ensi_key, icurr.event_date_time, icurr.operation_type, icurr.operation_pid,
							CASE WHEN i.is_prp = 1 AND s.service_name = 'Портал' THEN 1 ELSE 0 END is_prp
						FROM opd_cshd.ai_ensi_key_iteration i
						INNER JOIN opd_cshd.a_service s ON s.message_marker = i.message_marker
						INNER JOIN opd_cshd.ai_ensi_key_iteration icurr ON i.ensi_key = icurr.ensi_key
						WHERE i.progress = p_progress AND s.progress = p_progress AND icurr.progress = p_progress AND
							i.iteration = currentIteration AND icurr.iteration = currentIteration + 1
				) prev
				FULL OUTER JOIN
				(
					SELECT s.pk, s.message_marker, s.service_name, s.status_service, s.activation_date_time,
							s.source_service, i.ensi_key, i.event_date_time, i.operation_type, i.operation_pid,
							CASE WHEN i.is_prp = 1 AND s.service_name = 'Портал' THEN 1 ELSE 0 END is_prp
						FROM opd_cshd.ai_ensi_key_iteration i
						INNER JOIN opd_cshd.a_service s ON s.message_marker = i.message_marker
						WHERE i.progress = p_progress AND s.progress = p_progress AND i.iteration = currentIteration + 1
				) curr ON prev.ensi_key = curr.ensi_key AND prev.service_name = curr.service_name;
		END IF;
	END LOOP;
END;
$BODY$;
