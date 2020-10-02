DECLARE @count int = 0, @total int = 0, @i1 int = 0, @i2 int = 0, @i3 int = 0;
DECLARE @scount nvarchar(10), @stotal nvarchar(10), @si1 nvarchar(10), @si2 nvarchar(10), @si3 nvarchar(10), @msg nvarchar(max);

BEGIN TRANSACTION;

SELECT @count = COUNT(*) FROM {0};
SELECT @total = COUNT(*) FROM {0} WHERE State > 0 OR State_Tmp > 0;

INSERT INTO #t1 (ID, PatchID, Flag)
SELECT t.ID, e.PatchID, CASE WHEN t.State = t.State_Tmp THEN 1 ELSE 0 END
	FROM {0} t
	INNER JOIN N108_PATCH tp ON t.PatchID = tp.N108_ID
	CROSS APPLY
	(
		SELECT TOP 1 e.PatchID
			FROM {0} e
			INNER JOIN N108_PATCH p ON e.PatchID = p.N108_ID
			WHERE e.ObjectID = t.ObjectID AND p.N108_DATE_CONF IS NOT NULL
				AND
				(
					e.State > t.State
					OR
					e.State = 0
					OR
					(e.ObjectDel = 1 OR e.RemoveRight = 1 OR e.RemoveLeft = 1) AND e.State = t.State
				)
				AND
				(
					t.DocumentEndDate >= e.DocumentBeginDate AND t.DocumentBeginDate <= e.DocumentEndDate
					OR
					e.RemoveRight = 1 AND e.DocumentEndDate < t.DocumentBeginDate
					OR
					e.RemoveLeft = 1 AND e.DocumentBeginDate > t.DocumentEndDate
					OR
					e.ObjectDel = 1
				)
				AND
				(
					p.N108_DATE_CONF > tp.N108_DATE_CONF
					OR
					p.N108_ID = t.PatchID AND (e.ID > t.ID OR t.ObjectDel = 1 AND e.ID = t.ID OR t.AutomaticallyAdded = 1 AND e.ID < t.ID)
					OR
					p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID AND p.PATCH_NUMBER > tp.PATCH_NUMBER
				)
				-- Как мы выбираем нужную запись
				ORDER BY
					CASE
						WHEN p.N108_ID = t.PatchID THEN 0
						WHEN p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID THEN 1
						ELSE 2
						END,
					CASE
						WHEN p.N108_ID = t.PatchID THEN 0
						WHEN p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID THEN p.PATCH_NUMBER
						ELSE ROW_NUMBER() OVER(ORDER BY p.N108_DATE_CONF)
						END
	) e
	WHERE t.State > 0;

SET @i1 = @@ROWCOUNT;

INSERT INTO #t2 (ID, PatchID)
SELECT t.ID, e.PatchID
	FROM {0} t
	INNER JOIN N108_PATCH tp ON t.PatchID = tp.N108_ID
	CROSS APPLY
	(
		SELECT TOP 1 PatchID
			FROM {0} e
			INNER JOIN N108_PATCH p ON e.PatchID = p.N108_ID
			WHERE e.ObjectID = t.ObjectID AND p.N108_DATE_CONF IS NOT NULL
				AND 
				(
					e.State_Tmp > t.State_Tmp
					OR
					e.State_Tmp = 0
					OR
					(e.ObjectDel = 1 OR e.RemoveRight = 1 OR e.RemoveLeft = 1) AND e.State_Tmp = t.State_Tmp
				)
				AND
				(
					t.DocumentEndDate >= e.DocumentBeginDate AND t.DocumentBeginDate <= e.DocumentEndDate 
					OR
					e.RemoveRight = 1 AND e.DocumentEndDate < t.DocumentBeginDate
					OR
					e.RemoveLeft = 1 AND e.DocumentBeginDate > t.DocumentEndDate
					OR
					e.ObjectDel = 1
				)
				AND
				(
					p.N108_DATE_CONF > tp.N108_DATE_CONF
					OR
					p.N108_ID = t.PatchID AND (e.ID > t.ID OR t.ObjectDel = 1 AND e.ID = t.ID OR t.AutomaticallyAdded = 1 AND e.ID < t.ID)
					OR
					p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID AND p.PATCH_NUMBER > tp.PATCH_NUMBER
				)
				ORDER BY
					CASE
						WHEN p.N108_ID = t.PatchID THEN 0
						WHEN p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID THEN 1
						ELSE 2
						END,
					CASE
						WHEN p.N108_ID = t.PatchID THEN e.ID
						WHEN p.COMPLEXPATCH_GUID = tp.COMPLEXPATCH_GUID THEN p.PATCH_NUMBER
						ELSE ROW_NUMBER() OVER(ORDER BY p.N108_DATE_CONF)
						END
	) e
	WHERE t.State_Tmp > 0 AND (t.State IS NULL OR t.State != t.State_Tmp);

SET @i2 = @@ROWCOUNT;

IF @i1 + @i2 != @total
BEGIN
    INSERT INTO #t3 (ID)
    SELECT ID
        FROM {0}
        WHERE (State != State_Tmp OR State IS NULL AND State_Tmp > 0)
            AND ID NOT IN (SELECT ID FROM #t1) AND ID NOT IN (SELECT ID FROM #t2);

    SET @i3 = @@ROWCOUNT;
END;


SELECT @scount = CAST(@count as nvarchar(10)), @stotal = CAST(@total as nvarchar(10)), @si1 = CAST(@i1 as nvarchar(10)), @si2 = CAST(@i2 as nvarchar(10)), @si3 = CAST(@i3 as nvarchar(10));

IF @i1 + @i2 + @i3 != @total
BEGIN
	SET @msg = N'Ошибка конвертации раздела {0}: ' + @stotal + N'/' + @scount + N' (' + @si1 + N', ' + @si2 + N', ' + @si3 + N')';
	THROW 50000, @msg, 1;
END;

UPDATE e SET State = t.PatchID, State_Tmp = CASE WHEN t.Flag = 1 THEN t.PatchID ELSE e.State_Tmp END
	FROM {0} e
	INNER JOIN #t1 t ON t.ID = e.ID;

TRUNCATE TABLE #t1;

UPDATE e SET State_Tmp = t.PatchID
	FROM {0} e
	INNER JOIN #t2 t ON t.ID = e.ID;

TRUNCATE TABLE #t2;

IF @i1 + @i2 != @total
BEGIN
    UPDATE {0} SET State_Tmp = State
       WHERE ID IN (SELECT ID FROM #t3);
END;

TRUNCATE TABLE #t3;

COMMIT TRANSACTION;

SET @msg = N'Обработка раздела {0} успешно завершена: ' + @stotal + N'/' + @scount + N' (' + @si1 + N', ' + @si2 + N', ' + @si3 + N')' + N'-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------';
PRINT @msg;
GO


--