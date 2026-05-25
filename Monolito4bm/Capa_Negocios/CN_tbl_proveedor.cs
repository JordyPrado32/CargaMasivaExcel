using System;
using System.Collections.Generic;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    public class CN_tbl_proveedor
    {
        private const string TablaPendientes = "tbl_proveedor_reasignacion_pendiente";

        public static List<tbl_proveedor> Listar()
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_proveedor>()
                    .OrderByDescending(p => p.prov_id)
                    .ToList();
            }
        }

        public static List<tbl_proveedor> ListarActivos()
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_proveedor>()
                    .Where(p => p.prov_estado == 'A')
                    .OrderBy(p => p.prov_nombre)
                    .ToList();
            }
        }

        public static tbl_proveedor BuscarPorId(int id)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_proveedor>()
                    .FirstOrDefault(p => p.prov_id == id);
            }
        }

        public static bool ExisteNombre(string nombre, int idIgnorar = 0)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_proveedor>()
                    .Any(p => p.prov_nombre == nombre
                           && p.prov_estado == 'A'
                           && p.prov_id != idIgnorar);
            }
        }

        public static void Guardar(tbl_proveedor proveedor)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    AsegurarTablaPendientes(dc);

                    proveedor.prov_nombre = (proveedor.prov_nombre ?? string.Empty).Trim();
                    proveedor.prov_estado = 'A';
                    dc.GetTable<tbl_proveedor>().InsertOnSubmit(proveedor);
                    dc.SubmitChanges();

                    ReasignarProductosPendientes(dc, proveedor.prov_id, proveedor.prov_nombre);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar proveedor: " + ex.Message);
            }
        }

        public static void Modificar(tbl_proveedor proveedor)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var existente = dc.GetTable<tbl_proveedor>()
                        .FirstOrDefault(p => p.prov_id == proveedor.prov_id)
                        ?? throw new Exception("Proveedor no encontrado.");

                    existente.prov_nombre = (proveedor.prov_nombre ?? string.Empty).Trim();
                    dc.SubmitChanges();

                    AsegurarTablaPendientes(dc);
                    ReasignarProductosPendientes(dc, existente.prov_id, existente.prov_nombre);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar proveedor: " + ex.Message);
            }
        }

        public static void Activar(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var prov = dc.GetTable<tbl_proveedor>()
                        .FirstOrDefault(p => p.prov_id == id)
                        ?? throw new Exception("Proveedor no encontrado.");
                    prov.prov_estado = 'A';
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al activar proveedor: " + ex.Message);
            }
        }

        public static void EliminarLogico(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var prov = dc.GetTable<tbl_proveedor>()
                        .FirstOrDefault(p => p.prov_id == id)
                        ?? throw new Exception("Proveedor no encontrado.");
                    prov.prov_estado = 'I';
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desactivar proveedor: " + ex.Message);
            }
        }

        public static void EliminarFisico(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    AsegurarTablaPendientes(dc);

                    var prov = dc.GetTable<tbl_proveedor>()
                        .FirstOrDefault(p => p.prov_id == id)
                        ?? throw new Exception("Proveedor no encontrado.");

                    string nombreProveedor = (prov.prov_nombre ?? string.Empty).Trim();
                    var productosAfectados = dc.GetTable<tbl_producto>()
                        .Where(p => p.prov_id == id)
                        .ToList();

                    foreach (var producto in productosAfectados)
                    {
                        RegistrarPendiente(dc, producto.pro_id, nombreProveedor);
                        producto.prov_id = null;
                    }

                    dc.GetTable<tbl_proveedor>().DeleteOnSubmit(prov);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar proveedor: " + ex.Message);
            }
        }

        private static void AsegurarTablaPendientes(MonolitoDataContext dc)
        {
            dc.ExecuteCommand($@"
IF OBJECT_ID('dbo.{TablaPendientes}', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.{TablaPendientes}
    (
        pendiente_id INT IDENTITY(1,1) PRIMARY KEY,
        pro_id INT NOT NULL,
        proveedor_nombre NVARCHAR(150) NOT NULL,
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_{TablaPendientes}_fecha DEFAULT SYSDATETIME()
    );
END");
        }

        private static void RegistrarPendiente(MonolitoDataContext dc, int productoId, string proveedorNombre)
        {
            dc.ExecuteCommand($@"
IF NOT EXISTS (
    SELECT 1
    FROM dbo.{TablaPendientes}
    WHERE pro_id = {{0}} AND LTRIM(RTRIM(proveedor_nombre)) = {{1}}
)
BEGIN
    INSERT INTO dbo.{TablaPendientes} (pro_id, proveedor_nombre)
    VALUES ({{0}}, {{1}})
END", productoId, proveedorNombre);
        }

        private static void ReasignarProductosPendientes(MonolitoDataContext dc, int proveedorId, string proveedorNombre)
        {
            var productosPendientes = dc.ExecuteQuery<int>($@"
SELECT pro_id
FROM dbo.{TablaPendientes}
WHERE LTRIM(RTRIM(proveedor_nombre)) = {{0}}", proveedorNombre).ToList();

            if (!productosPendientes.Any()) return;

            var productos = dc.GetTable<tbl_producto>()
                .Where(p => productosPendientes.Contains(p.pro_id) && !p.prov_id.HasValue)
                .ToList();

            foreach (var producto in productos)
            {
                producto.prov_id = proveedorId;
            }

            dc.SubmitChanges();

            dc.ExecuteCommand($@"
DELETE FROM dbo.{TablaPendientes}
WHERE LTRIM(RTRIM(proveedor_nombre)) = {{0}}", proveedorNombre);
        }
    }
}
