-- Точность временных меток - до секунды

DROP TABLE IF EXISTS Customer;
DROP TABLE IF EXISTS Purchase;

CREATE TABLE Customer
(
    Id int not null,
    RegistrationTs datetime2(0) not null
);

-- Purchase.Id является суррогатом и не используется в задаче
CREATE TABLE Purchase
(
    Id int not null identity(1, 1),
    CustomerId int not null,
    PayTs datetime2(0) null, -- Учитываем только оплаченные покупки
	PayIntervalInMin int null -- Количество минут, прошедших с момента регистрации покупателя
);
GO


DECLARE @customerCount int = 100000, -- Исходное значение 1 млн, но сервис на этом значении падает
    @customerId int = 0,
    @purchaseCount int = 3, -- Каждый покупатель сделал не более 3 покупок
    @purchaseNumber int = 0,
	@secIn25Days float = 25.0 * 24 * 60 * 60, -- Покупатели регистрировались и делали покупки в течение 25 дней
    @startTs datetime2(0) = '2020-05-01 00:00:00', -- Начали 1 мая 2020 года
	@customerRegTs datetime2(0),
    @purchasePayTs datetime2(0),
    @purchasePayIntervalInMin int,
    @patchSize int = 50000; -- Транзакции по 50 тысяч покупателей
    
-- Считаем, что покупка, получающая при генерации время оплаты
-- в пределах 5 минут до наступления 26 мая, ещё не оплачена
DECLARE @endTs datetime2(0) = DATEADD(minute, -5, DATEADD(day, 25, @startTs));

-- Организуем вставку транзакциями по 50 тысяч покупателей
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;
PRINT N'BEGIN TRANSACTION';

WHILE (@customerId < @customerCount)
BEGIN
	SET @customerId += 1;
	SET @customerRegTs = DATEADD(ms, CAST(RAND() * @secIn25Days as int), @startTs);
	INSERT INTO Customer (Id, RegistrationTs) VALUES (@customerId, @customerRegTs);

	SET @purchaseNumber = 0;
	WHILE (@purchaseNumber < @purchaseCount)
	BEGIN
		SET @purchasePayTs = DATEADD(ms, CAST(RAND() * @secIn25Days as bigint), @startTs);
		-- Покупка может быть совершена только после регистрации пользователя
        IF (@purchasePayTs > @customerRegTs)
		BEGIN
			IF (@purchasePayTs >= @endTs) -- Эти покупки ещё не оплачены
			BEGIN
				SET @purchasePayTs = NULL;
				SET @purchasePayIntervalInMin = NULL;
			END
			ELSE
			BEGIN
				SET @purchasePayIntervalInMin = DATEDIFF(minute, @customerRegTs, @purchasePayTs);
			END;

			INSERT INTO Purchase (CustomerId, PayTs, PayIntervalInMin) VALUES (@customerId, @purchasePayTs, @purchasePayIntervalInMin);
		END;

		SET @purchaseNumber += 1;
	END;

	IF ((@customerId % @patchSize) = 0)
	BEGIN
		COMMIT TRANSACTION;
		PRINT N'COMMIT TRANSACTION ' + CAST (@customerId as nvarchar(10));

		IF (@customerId != @customerCount)
		BEGIN
			BEGIN TRANSACTION;
		END;
	END;
END;

IF (@@TRANCOUNT > 0)
BEGIN
	COMMIT TRANSACTION;
END;
GO

CREATE UNIQUE CLUSTERED INDEX IX_Customer_Id ON Customer (Id);
-- Это будет наиболее часто используемое условие поиска
CREATE UNIQUE CLUSTERED INDEX IX_Purchase_CustomerId ON Purchase (CustomerId, Id);
-- Индекс для тривиального решения
CREATE NONCLUSTERED INDEX IX_Purchase_CustomerId_PayTs ON Purchase (CustomerId, PayTs);

-- Индексы для варианта со сбором дополнительных метрик
CREATE NONCLUSTERED INDEX IX_Purchase_Interval2 ON Purchase (CustomerId, PayIntervalInMin);
CREATE NONCLUSTERED INDEX IX_Purchase_Interval3 ON Purchase (CustomerId) WHERE PayIntervalInMin <= 7200;
GO





-- Простейшее решение в лоб
-- Создаём очевидный кластерный индекс на Customer и индекс для поиска нужных покупок в Purchase
-- CREATE UNIQUE CLUSTERED INDEX IX_Customer_Id ON Customer (Id);
-- CREATE NONCLUSTERED INDEX IX_Purchase_CustomerId_PayTs ON Purchase (CustomerId, PayTs);
-- На данном сервисе можно наблюдать Merge Join, но без ограничений параллелизма
-- для запроса генерируется параллельный план, который выполняется как Hash Join.

SELECT COUNT(*)
  FROM Customer c
  WHERE EXISTS(
    SELECT *
      FROM Purchase p
      WHERE p.CustomerId = c.Id AND DATEADD(minute, 5 * 24 * 60, c.RegistrationTs) > p.PayTs)

-- Не стоит допускать такую активность на ключевых таблицах.
-- Поскольку при создании записи о покупке не составляет труда собрать дополнительные метрики, предлагаю их собирать.
-- В данном случае не имеет смысла заводить для них отдельные таблицы,
-- хотя в рабочей системе это стоило бы сделать. Поэтому денормализуем таблицу Purchase и добавляем в неё
-- поле PayIntervalInMin, которое хранит количество минут с момента регистрации покупателя.
-- Такой точности для этих данных будет достаточно.
-- В этом случае запрос сводится к скану по одному некластерному индексу. Индекс выглядит так:
-- CREATE NONCLUSTERED INDEX IX_Purchase_Interval2 ON Purchase (CustomerId, PayIntervalInMin);
-- Первым полем является CustomerId, поскольку это позволяет эффективно выполнить агрегацию.

SELECT COUNT(DISTINCT(p.CustomerId))
  FROM Purchase p
  WHERE p.PayIntervalInMin <= 7200

-- Если никакие другие отчеты по времени покупки с момента регистрации нам не требуются,
-- можно реализовать ещё более эффективный индекс:
-- CREATE NONCLUSTERED INDEX IX_Purchase_Interval3 ON Purchase (CustomerId) WHERE PayIntervalInMin <= 7200;
-- Тогда запрос нужно будет немного изменить:

SELECT COUNT(DISTINCT(p.CustomerId))
  FROM Purchase p with(index(IX_Purchase_Interval3))
  WHERE p.PayIntervalInMin <=7200

-- Также, если все метрики заранее известны, имело бы смысл при создании покупки
-- выполнять дополнительную агрегацию. Например, устанавливать троичный флаг в специальной таблице.
-- Тогда при обращении к такой таблице нам не требовалась бы дополнительная агрегация.
-- Но в данном примере, как мне кажется, этого не требуется.