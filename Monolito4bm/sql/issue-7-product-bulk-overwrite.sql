BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Limpiar referencias de fotos antes de tocar productos.
    DELETE FROM dbo.tbl_pro_fotos;

    -- 2. Vaciar productos y reiniciar la identidad.
    DELETE FROM dbo.tbl_producto;
    DBCC CHECKIDENT ('dbo.tbl_producto', RESEED, 0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
