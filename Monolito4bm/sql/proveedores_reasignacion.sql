IF OBJECT_ID('dbo.tbl_proveedor_reasignacion_pendiente', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_proveedor_reasignacion_pendiente
    (
        pendiente_id INT IDENTITY(1,1) PRIMARY KEY,
        pro_id INT NOT NULL,
        proveedor_nombre NVARCHAR(150) NOT NULL,
        fecha_registro DATETIME2 NOT NULL
            CONSTRAINT DF_tbl_proveedor_reasignacion_pendiente_fecha DEFAULT SYSDATETIME()
    );
END
GO

CREATE OR ALTER TRIGGER dbo.tr_tbl_proveedor_reasignar_en_insert
ON dbo.tbl_proveedor
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.prov_id = i.prov_id
    FROM dbo.tbl_producto p
    INNER JOIN dbo.tbl_proveedor_reasignacion_pendiente rp
        ON rp.pro_id = p.pro_id
    INNER JOIN inserted i
        ON LTRIM(RTRIM(rp.proveedor_nombre)) = LTRIM(RTRIM(i.prov_nombre))
    WHERE p.prov_id IS NULL;

    DELETE rp
    FROM dbo.tbl_proveedor_reasignacion_pendiente rp
    INNER JOIN inserted i
        ON LTRIM(RTRIM(rp.proveedor_nombre)) = LTRIM(RTRIM(i.prov_nombre));
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_eliminar_proveedor_permanente
    @prov_id INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @prov_nombre NVARCHAR(150);

    SELECT @prov_nombre = prov_nombre
    FROM dbo.tbl_proveedor
    WHERE prov_id = @prov_id;

    IF @prov_nombre IS NULL
    BEGIN
        RAISERROR('Proveedor no encontrado.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.tbl_proveedor_reasignacion_pendiente (pro_id, proveedor_nombre)
    SELECT p.pro_id, @prov_nombre
    FROM dbo.tbl_producto p
    WHERE p.prov_id = @prov_id
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.tbl_proveedor_reasignacion_pendiente rp
          WHERE rp.pro_id = p.pro_id
            AND LTRIM(RTRIM(rp.proveedor_nombre)) = LTRIM(RTRIM(@prov_nombre))
      );

    UPDATE dbo.tbl_producto
    SET prov_id = NULL
    WHERE prov_id = @prov_id;

    DELETE FROM dbo.tbl_proveedor
    WHERE prov_id = @prov_id;
END
GO
