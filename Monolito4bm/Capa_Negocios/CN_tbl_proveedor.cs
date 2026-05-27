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

        public static List<tbl_proveedor> Buscar(string nombre = null, char? estado = null)
        {
            using (var dc = new MonolitoDataContext())
            {
                var query = dc.GetTable<tbl_proveedor>().AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    string termino = nombre.Trim();
                    query = query.Where(p => p.prov_nombre.Contains(termino));
                }

                if (estado.HasValue)
                {
                    query = query.Where(p => p.prov_estado == estado.Value);
                }

                return query
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

        public static List<ProveedorCargaFila> LeerArchivoCargaMasiva(byte[] contenidoArchivo, string nombreArchivo)
        {
            return ProveedorCargaMasivaParser.Leer(contenidoArchivo, nombreArchivo);
        }

        public static ResultadoCargaProveedores ProcesarCargaMasiva(IEnumerable<ProveedorCargaFila> filas, TipoInsercionProveedor tipoInsercion)
        {
            var filasNormalizadas = NormalizarFilasCarga(filas, tipoInsercion);

            var resultado = new ResultadoCargaProveedores
            {
                FilasProcesadas = filasNormalizadas.Count
            };

            using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required,
                new System.Transactions.TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                }))
            using (var dc = new MonolitoDataContext())
            {
                AsegurarTablaPendientes(dc);

                if (tipoInsercion == TipoInsercionProveedor.ReemplazarTodo)
                {
                    EjecutarReemplazoTotal(dc, filasNormalizadas, resultado);
                }
                else
                {
                    EjecutarCargaIncremental(dc, filasNormalizadas, resultado);
                }

                scope.Complete();
            }

            return resultado;
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

        private static List<ProveedorCargaFilaNormalizada> NormalizarFilasCarga(IEnumerable<ProveedorCargaFila> filas, TipoInsercionProveedor tipoInsercion)
        {
            var normalizadas = (filas ?? Enumerable.Empty<ProveedorCargaFila>())
                .Select(f => new ProveedorCargaFilaNormalizada
                {
                    NumeroFilaArchivo = f.NumeroFilaArchivo,
                    ProveedorId = f.ProveedorId,
                    NombreProveedor = (f.NombreProveedor ?? string.Empty).Trim(),
                    EstadoProveedor = f.EstadoProveedor == 'I' ? 'I' : 'A',
                    NombreNormalizado = NormalizarNombreProveedor(f.NombreProveedor)
                })
                .ToList();

            if (!normalizadas.Any())
            {
                throw new Exception("No hay filas preparadas para importar.");
            }

            var filasSinNombre = normalizadas.Where(f => string.IsNullOrWhiteSpace(f.NombreProveedor)).ToList();
            if (filasSinNombre.Any())
            {
                throw new Exception($"La fila {filasSinNombre[0].NumeroFilaArchivo} no tiene nombre de proveedor.");
            }

            var idsDuplicados = normalizadas
                .Where(f => f.ProveedorId.HasValue)
                .GroupBy(f => f.ProveedorId.Value)
                .FirstOrDefault(g => g.Count() > 1);

            if (idsDuplicados != null)
            {
                throw new Exception($"El archivo tiene IDs repetidos. Revisa el proveedor con ID {idsDuplicados.Key}.");
            }

            var nombresDuplicados = normalizadas
                .GroupBy(f => f.NombreNormalizado)
                .FirstOrDefault(g => g.Count() > 1);

            if (nombresDuplicados != null)
            {
                throw new Exception($"El archivo tiene nombres repetidos. Revisa el proveedor '{nombresDuplicados.First().NombreProveedor}'.");
            }

            return normalizadas;
        }

        private static void EjecutarCargaIncremental(MonolitoDataContext dc, List<ProveedorCargaFilaNormalizada> filas, ResultadoCargaProveedores resultado)
        {
            var proveedoresActuales = dc.GetTable<tbl_proveedor>().ToList();
            var porId = proveedoresActuales.ToDictionary(p => p.prov_id);
            var porNombre = proveedoresActuales
                .GroupBy(p => NormalizarNombreProveedor(p.prov_nombre))
                .ToDictionary(g => g.Key, g => g.First());

            var insertarConId = new List<ProveedorCargaFilaNormalizada>();
            var insertarSinId = new List<ProveedorCargaFilaNormalizada>();

            foreach (var fila in filas)
            {
                tbl_proveedor existente = null;

                if (fila.ProveedorId.HasValue)
                {
                    porId.TryGetValue(fila.ProveedorId.Value, out existente);
                }

                if (existente == null)
                {
                    porNombre.TryGetValue(fila.NombreNormalizado, out existente);
                }

                if (existente != null)
                {
                    bool estabaInactivo = existente.prov_estado == 'I';
                    bool huboCambios = false;

                    if (!string.Equals((existente.prov_nombre ?? string.Empty).Trim(), fila.NombreProveedor, StringComparison.Ordinal))
                    {
                        existente.prov_nombre = fila.NombreProveedor;
                        huboCambios = true;
                    }

                    if ((existente.prov_estado ?? 'A') != fila.EstadoProveedor)
                    {
                        existente.prov_estado = fila.EstadoProveedor;
                        huboCambios = true;
                    }

                    if (huboCambios)
                    {
                        resultado.Actualizados++;
                        if (estabaInactivo && fila.EstadoProveedor == 'A')
                        {
                            resultado.Reactivados++;
                        }
                    }

                    continue;
                }

                if (fila.ProveedorId.HasValue)
                {
                    insertarConId.Add(fila);
                }
                else
                {
                    insertarSinId.Add(fila);
                }
            }

            if (proveedoresActuales.Any(p =>
                filas.Any(f => f.ProveedorId.HasValue && f.ProveedorId.Value != p.prov_id && f.NombreNormalizado == NormalizarNombreProveedor(p.prov_nombre))))
            {
                throw new Exception("El archivo intenta insertar un proveedor con un nombre ya existente y otro ID distinto.");
            }

            dc.SubmitChanges();

            InsertarProveedoresConId(dc, insertarConId);
            resultado.Insertados += insertarConId.Count;

            if (insertarSinId.Any())
            {
                var nuevos = insertarSinId.Select(f => new tbl_proveedor
                {
                    prov_nombre = f.NombreProveedor,
                    prov_estado = f.EstadoProveedor
                }).ToList();

                dc.GetTable<tbl_proveedor>().InsertAllOnSubmit(nuevos);
                dc.SubmitChanges();
                resultado.Insertados += nuevos.Count;
            }
        }

        private static void EjecutarReemplazoTotal(MonolitoDataContext dc, List<ProveedorCargaFilaNormalizada> filas, ResultadoCargaProveedores resultado)
        {
            var asignacionesPrevias = dc.GetTable<tbl_producto>()
                .Where(p => p.prov_id.HasValue)
                .Select(p => new
                {
                    p.pro_id,
                    ProveedorId = p.prov_id.Value
                })
                .ToList();

            var productosConProveedor = dc.GetTable<tbl_producto>()
                .Where(p => p.prov_id.HasValue)
                .ToList();

            foreach (var producto in productosConProveedor)
            {
                producto.prov_id = null;
            }

            dc.SubmitChanges();

            dc.ExecuteCommand($"DELETE FROM dbo.{TablaPendientes}");

            var proveedores = dc.GetTable<tbl_proveedor>().ToList();
            if (proveedores.Any())
            {
                dc.GetTable<tbl_proveedor>().DeleteAllOnSubmit(proveedores);
                dc.SubmitChanges();
            }

            dc.ExecuteCommand("DBCC CHECKIDENT ('dbo.tbl_proveedor', RESEED, 0)");
            var idsNuevos = InsertarProveedoresSinConservarId(dc, filas);
            resultado.Insertados = filas.Count;

            var idsCargados = idsNuevos
                .ToHashSet();

            var productoIdsReasignables = asignacionesPrevias
                .Where(a => idsCargados.Contains(a.ProveedorId))
                .Select(a => a.pro_id)
                .ToList();

            var productosAReasignar = dc.GetTable<tbl_producto>()
                .Where(p => !p.prov_id.HasValue && productoIdsReasignables.Contains(p.pro_id))
                .ToList();

            foreach (var producto in productosAReasignar)
            {
                int proveedorId = asignacionesPrevias
                    .Where(a => a.pro_id == producto.pro_id)
                    .Select(a => a.ProveedorId)
                    .First();
                producto.prov_id = proveedorId;
            }

            dc.SubmitChanges();

            resultado.ProductosReasignados = productosAReasignar.Count;
            resultado.ProductosSinProveedor = asignacionesPrevias.Count - productosAReasignar.Count;
        }

        private static void InsertarProveedoresConId(MonolitoDataContext dc, IEnumerable<ProveedorCargaFilaNormalizada> filas)
        {
            var lista = (filas ?? Enumerable.Empty<ProveedorCargaFilaNormalizada>()).ToList();
            if (!lista.Any())
            {
                return;
            }

            dc.ExecuteCommand("SET IDENTITY_INSERT dbo.tbl_proveedor ON");

            try
            {
                foreach (var fila in lista)
                {
                    if (!fila.ProveedorId.HasValue)
                    {
                        throw new Exception($"La fila {fila.NumeroFilaArchivo} no tiene ID y no puede insertarse preservando identidad.");
                    }

                    dc.ExecuteCommand(
                        "INSERT INTO dbo.tbl_proveedor (prov_id, prov_nombre, prov_estado) VALUES ({0}, {1}, {2})",
                        fila.ProveedorId.Value,
                        fila.NombreProveedor,
                        fila.EstadoProveedor);
                }
            }
            finally
            {
                dc.ExecuteCommand("SET IDENTITY_INSERT dbo.tbl_proveedor OFF");
            }
        }

        private static List<int> InsertarProveedoresSinConservarId(MonolitoDataContext dc, IEnumerable<ProveedorCargaFilaNormalizada> filas)
        {
            var lista = (filas ?? Enumerable.Empty<ProveedorCargaFilaNormalizada>()).ToList();
            var idsGenerados = new List<int>();

            if (!lista.Any())
            {
                return idsGenerados;
            }

            foreach (var fila in lista)
            {
                var proveedor = new tbl_proveedor
                {
                    prov_nombre = fila.NombreProveedor,
                    prov_estado = fila.EstadoProveedor
                };

                dc.GetTable<tbl_proveedor>().InsertOnSubmit(proveedor);
                dc.SubmitChanges();
                idsGenerados.Add(proveedor.prov_id);
            }

            return idsGenerados;
        }

        private static string NormalizarNombreProveedor(string nombre)
        {
            return (nombre ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
