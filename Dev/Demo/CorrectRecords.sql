CREATE FUNCTION dbo.GetColumnNameForGuid (@guid uniqueidentifier)
RETURNS nvarchar(32)
BEGIN
	DECLARE @bguid binary(16), @b1 binary(8), @b2 binary(8), @bi1 bigint, @bi2 bigint, @biu1 bigint, @biu2 bigint;
	SET @bguid = CAST(@guid as binary(16))
	SET @b1 = SUBSTRING(@bguid, 1, 8);
	SET @b2 = SUBSTRING(@bguid, 9, 8);
	SET @biu1 = CAST(SUBSTRING(@b1, 8, 1) as bigint);
	SET @bi1 =
		IIF(@biu1 > 127, -256 + @biu1, @biu1) * 256 * 256 * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b1, 7, 1) as bigint) * 256 * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b1, 6, 1) as bigint) * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b1, 5, 1) as bigint) * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b1, 4, 1) as bigint) * 256 * 256 * 256 +
		CAST(SUBSTRING(@b1, 3, 1) as bigint) * 256 * 256 +
		CAST(SUBSTRING(@b1, 2, 1) as bigint) * 256 +
		CAST(SUBSTRING(@b1, 1, 1) as bigint);
	SET @biu2 = CAST(SUBSTRING(@b2, 8, 1) as bigint);
	SET @bi2 =
		IIF(@biu2 > 127, -256 + @biu2, @biu2) * 256 * 256 * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b2, 7, 1) as bigint) * 256 * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b2, 6, 1) as bigint) * 256 * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b2, 5, 1) as bigint) * 256 * 256 * 256 * 256 +
		CAST(SUBSTRING(@b2, 4, 1) as bigint) * 256 * 256 * 256 +
		CAST(SUBSTRING(@b2, 3, 1) as bigint) * 256 * 256 +
		CAST(SUBSTRING(@b2, 2, 1) as bigint) * 256 +
		CAST(SUBSTRING(@b2, 1, 1) as bigint);
	RETURN N'C' + CAST(ABS(@bi1 ^ @bi2) as nvarchar(32));
END;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @elementId int, @tableName sysname;

DECLARE curs CURSOR FAST_FORWARD FOR
	SELECT e.N109_ID, t.name
		FROM N109_ELEMENT e
		INNER JOIN sys.tables t ON t.name = N'E' + CAST(e.N109_ID as nvarchar(10))
		WHERE e.N109_ID NOT IN (1953, 2407, 11500); -- Удаляем из выборки системные справочники
OPEN curs;
FETCH NEXT FROM curs INTO @elementID, @tableName;
WHILE @@FETCH_STATUS != -1
BEGIN
	DECLARE @sql1 nvarchar(max), @changes int = 0;

	PRINT N'ТАБЛИЦА: ' + CAST(@elementId as nvarchar(10)) + N', ' + @tableName;


	-- Корректируем неправильно заданные заглушки, созданные плагином для заполнения разрывов
	-- Сознательно не копируем множественные связи и строки дедупликации, поскольку это чисто технические записи, и в принципе этого не требуют
	SET @sql1 = N'UPDATE ' + @tableName + N' SET DocumentBeginDate = DATEADD(ms, 997, DocumentBeginDate)
		WHERE DATEPART(hour, DocumentBeginDate) = 23 AND DATEPART(minute, DocumentBeginDate) = 59 AND DATEPART(second, DocumentBeginDate) = 59 AND DATEPART(ms, DocumentBeginDate) = 3';
	EXECUTE sp_executesql @statement = @sql1;
	SET @changes = @@ROWCOUNT;
	
	IF (@changes > 0)
	BEGIN
		-- Поскольку у таких записей нет ни строк дедупликации, ни множественных связей, мы можем их просто удалить
		SET @sql1 = N'DELETE FROM ' + @tableName + N' WHERE DocumentBeginDate >= DocumentEndDate';
		EXECUTE sp_executesql @statement = @sql1;
		PRINT N'Исправлено некорректных записей с флагом "Объект не действует": ' + CAST(@changes as nvarchar(10));
	END;


	CREATE TABLE #t1
	(
		OriginId int NOT NULL,
		OriginGuid uniqueidentifier NOT NULL,
		Id int NOT NULL PRIMARY KEY CLUSTERED,
		Guid uniqueidentifier NOT NULL,
		OriginLinkGuid uniqueidentifier NULL,
		LinkGuid uniqueidentifier NULL,
	);

	CREATE NONCLUSTERED INDEX IX_1 ON #t1 (OriginId);
	CREATE NONCLUSTERED INDEX IX_2 ON #t1 (OriginLinkGuid, OriginGuid) INCLUDE (LinkGuid, Guid);

	SET @sql1 = N'INSERT INTO #t1 (Id, Guid, OriginId, OriginGuid)
		SELECT e1.Id, e1.Guid, COALESCE(e2.Id, e3.Id, e4.Id, e5.Id), COALESCE(e2.Guid, e3.Guid, e4.Guid, e5.Guid)
			FROM ' + @tableName + N' e1
			OUTER APPLY
			(
				SELECT TOP (1) Id, Guid
					FROM ' + @tableName + N' e2
					WHERE e1.ObjectID = e2.ObjectId AND e1.DocumentBeginDate = e2.DocumentBeginDate AND
						(
							e1.State_Tmp IS NULL AND e2.State = 0
							OR
							e1.State IS NULL AND e1.State_Tmp = 0 AND e2.State = 0
							OR
							e1.State IS NOT NULL AND IIF(e1.State = 0, (SELECT MAX(e3.State) FROM ' + @tableName + N' e3 WHERE e3.ObjectID = e2.ObjectId AND e3.State IS NOT NULL), e1.State - 1) = e2.State
						)
					ORDER BY Id DESC
			) e2
			OUTER APPLY
			(
				SELECT TOP (1) Id, Guid
					FROM ' + @tableName + N' e3
					WHERE e1.ObjectID = e3.ObjectId AND e1.DocumentBeginDate = e3.DocumentBeginDate AND e3.State IS NOT NULL AND e3.Id < e1.Id
					ORDER BY Id DESC
			) e3
			OUTER APPLY
			(
				SELECT TOP (1) Id, Guid
					FROM ' + @tableName + N' e4
					WHERE e1.ObjectID = e4.ObjectId AND e1.DocumentBeginDate = e4.DocumentBeginDate AND e4.State_Tmp IS NOT NULL AND e4.Id < e1.Id
					ORDER BY Id DESC
			) e4
			OUTER APPLY
			(
				SELECT TOP (1) Id, Guid
					FROM ' + @tableName + N' e5
					WHERE e1.ObjectID = e5.ObjectId AND e1.DocumentBeginDate = e5.DocumentBeginDate AND e5.Id < e1.Id
					ORDER BY Id DESC
			) e5
			WHERE DATEPART(hour, e1.DocumentEndDate) = 23 AND DATEPART(minute, e1.DocumentEndDate) = 59 AND DATEPART(second, e1.DocumentEndDate) = 59 AND DATEPART(ms, e1.DocumentEndDate) = 0';
	EXECUTE sp_executesql @statement = @sql1;
	SET @changes = @@ROWCOUNT;
	
	IF (@changes > 0)
	BEGIN
		SET @sql1 = N'UPDATE ' + @tableName + N' SET DocumentEndDate = DATEADD(ms, 997, DocumentEndDate)
			WHERE Id IN (SELECT Id FROM #t1)';
		EXECUTE sp_executesql @statement = @sql1;

		SET @sql1 = N'INSERT INTO ' + @tableName + N'_DEDUP (ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3)
			SELECT t.ID, d.DEDUP_STR_1, d.DEDUP_STR_2, d.DEDUP_STR_3
				FROM ' + @tableName + N'_DEDUP d INNER JOIN #t1 t ON t.OriginId = d.ID
				WHERE NOT EXISTS (SELECT * FROM ' + @tableName + N'_DEDUP ed WHERE ed.ID = t.ID)';
		EXECUTE sp_executesql @statement = @sql1;


		DECLARE @colId int, @colGuid uniqueidentifier, @colName nvarchar(32);
		DECLARE mlcurs CURSOR FAST_FORWARD FOR
			SELECT c.N112_ID, c.N112_GUID, dbo.GetColumnNameForGuid(c.N112_GUID)
				FROM N141_COL_LINK l
				INNER JOIN N112_COL_EXT c ON l.N109_EXTCOL_ID_REF = c.N112_ID
				WHERE l.N141_MULTIPLE_LINK = 1 AND l.N109_ID_REF = @elementId;
		OPEN mlcurs;
		FETCH NEXT FROM mlcurs INTO @colId, @colGuid, @colName;
		WHILE @@FETCH_STATUS != -1
		BEGIN
			SET @sql1 = N'UPDATE #t1 SET OriginLinkGuid = e.' + @colName + N', LinkGuid = IIF(e.' + @colName + N' IS NOT NULL, NEWID(), NULL)
					FROM #t1 t
					INNER JOIN ' + @tableName + N' e ON e.ID = t.OriginId';
			EXECUTE sp_executesql @statement = @sql1;

			SET @sql1 = N'UPDATE e
				SET ' + @colName + N' = t.LinkGuid
				FROM ' + @tableName + N' e
				INNER JOIN #t1 t ON t.Id = e.Id'
			EXECUTE sp_executesql @statement = @sql1;

			SET @sql1 = N'INSERT INTO MULTIPLELINK (LinkGuid, RefPositionGuid, RefExtColID, AncObjectID)
				SELECT t.LinkGuid, t.Guid, ' + CAST(@colId as nvarchar(10)) + N', ml.AncObjectID
					FROM #t1 t
					LEFT JOIN MULTIPLELINK ml ON t.OriginLinkGuid = ml.LinkGuid AND t.OriginGuid = ml.RefPositionGuid';
			EXECUTE sp_executesql @statement = @sql1;

			SET @colId = NULL;
			SET @colGuid = NULL;
			SET @colName = NULL;

			FETCH NEXT FROM mlcurs INTO @colId, @colGuid, @colName;
		END;
		CLOSE mlcurs;
		DEALLOCATE mlcurs;
		
		PRINT N'Исправлено некорректных записей, полученных в результате автоматического аннулирования: ' + CAST(@changes as nvarchar(10));
	END;

	DROP TABLE #t1;

	--PRINT '';
	--PRINT '';

	SET @elementId = NULL;
	SET @tableName = NULL;

	FETCH NEXT FROM curs INTO @elementID, @tableName;
END;
CLOSE curs;
DEALLOCATE curs;

COMMIT TRANSACTION;
GO

DROP FUNCTION dbo.GetColumnNameForGuid;
GO
