CREATE TABLE B
(
  ID NUMBER NOT NULL PRIMARY KEY,
  D BLOB
)
/

CREATE OR REPLACE PROCEDURE BIG_BLOB_ADD(
p_ID NUMBER,
p_DATA BLOB)
IS
BEGIN
    UPDATE B SET D = p_DATA WHERE ID = p_ID;   
END;
/

CREATE OR REPLACE PROCEDURE BIG_BLOB_APPEND(
p_ID NUMBER,
p_DATA BLOB)
IS
begin
    declare v_blob blob;
    begin
        select D into v_blob from B where ID = p_ID for update;
        DBMS_LOB.APPEND(v_blob, p_DATA);

        COMMIT;
    end;
end;
/

CREATE OR REPLACE PROCEDURE BIG_BLOB_LOAD(
p_ID NUMBER,
p_START_POS in INTEGER,
p_LENGTH in INTEGER,
p_DATA in out BLOB,
p_DATA_LENGTH out INTEGER)
AS
tmpBlob BLOB;
blobLength INTEGER;
cnt INTEGER;
BEGIN
    p_DATA_LENGTH := 0;
    select count(*) into cnt from B where ID = p_ID; 
    
    if cnt > 0
    then
        select length(D) into blobLength from B where ID = p_ID;
    
        if p_START_POS <= blobLength
        then
            if blobLength - p_START_POS + 1 > p_LENGTH
            then
                p_DATA_LENGTH := p_LENGTH;
            else
                p_DATA_LENGTH := blobLength - p_START_POS + 1;
            end if;
            select D into tmpBlob from B where ID = p_ID;

            dbms_lob.copy(p_DATA, tmpBlob, p_LENGTH, 1, p_START_POS);
        else
            p_DATA := NULL;
        end if;
    else
        p_DATA := NULL;
    end if;
END;
/

CREATE OR REPLACE PROCEDURE BIG_BLOB_SAVE (
    p_ID NUMBER,
    p_c OUT SYS_REFCURSOR)
IS
begin
    DECLARE
    c int;
    begin
        BEGIN
            SELECT id into c FROM b WHERE id = 1 for update;
        EXCEPTION
        WHEN no_data_found THEN
            INSERT INTO B (ID, D) VALUES (1, HEXTORAW('0'));
        END;
        
        BEGIN
            SELECT id into c FROM b WHERE id = 1 AND d IS NOT NULL for update;
        EXCEPTION
        WHEN no_data_found THEN
            UPDATE B SET D = HEXTORAW('0') WHERE id = 1;
        END;
    end;
    
    open p_c for SELECT id, d FROM B WHERE id = p_ID;

end;
/

INSERT INTO B (ID, D) VALUES (1, NULL);
/
COMMIT;
/
