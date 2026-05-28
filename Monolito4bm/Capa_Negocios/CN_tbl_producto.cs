using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    public class CN_tbl_producto
    {
        private static IEnumerable<tbl_producto> QueryConRelaciones()
        {
            using (var dc = new MonolitoDataContext())
            {
                var datosBD = dc.GetTable<tbl_producto>()
                    .OrderByDescending(p => p.pro_id)
                    .Select(p => new
                    {
                        p.pro_id,
                        p.pro_nombre,
                        p.pro_cantidad,
                        p.pro_precio,
                        p.pro_estado,
                        p.prov_id,
                        prov_nombre = p.tbl_proveedor != null ? p.tbl_proveedor.prov_nombre : null,
                        fotos = p.tbl_pro_fotos
                                  .Select(f => new { f.foto_id, f.pro_id, f.foto_ruta, f.fecha_subida, f.foto_estado })
                                  .ToList()
                    }).ToList();

                return datosBD.Select(p =>
                {
                    var prod = new tbl_producto
                    {
                        pro_id = p.pro_id,
                        pro_nombre = p.pro_nombre,
                        pro_cantidad = p.pro_cantidad,
                        pro_precio = p.pro_precio,
                        pro_estado = p.pro_estado,
                        prov_id = p.prov_id,
                        tbl_proveedor = new tbl_proveedor { prov_nombre = string.IsNullOrWhiteSpace(p.prov_nombre) ? "Sin proveedor" : p.prov_nombre }
                    };
                    var set = new EntitySet<tbl_pro_fotos>();
                    set.AddRange(p.fotos.Select(f => new tbl_pro_fotos
                    {
                        foto_id = f.foto_id,
                        pro_id = f.pro_id,
                        foto_ruta = f.foto_ruta,
                        fecha_subida = f.fecha_subida,
                        foto_estado = f.foto_estado
                    }));
                    prod.tbl_pro_fotos = set;
                    return prod;
                }).ToList();
            }
        }

        public static List<tbl_producto> Listar()
            => QueryConRelaciones().ToList();

        public static tbl_producto BuscarPorId(int id)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_producto>().FirstOrDefault(p => p.pro_id == id);
            }
        }

        public static List<tbl_producto> Buscar(
            string nombre = null,
            int? proveedorId = null,
            char? estado = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            int? stockMin = null,
            int? stockMax = null)
        {
            using (var dc = new MonolitoDataContext())
            {
                var q = dc.GetTable<tbl_producto>().AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombre))
                    q = q.Where(p => p.pro_nombre.Contains(nombre));
                if (proveedorId.HasValue && proveedorId.Value == -1)
                    q = q.Where(p => !p.prov_id.HasValue);
                else if (proveedorId.HasValue)
                    q = q.Where(p => p.prov_id == proveedorId.Value);
                if (estado.HasValue)
                    q = q.Where(p => p.pro_estado == estado.Value);
                if (precioMin.HasValue)
                    q = q.Where(p => p.pro_precio >= precioMin.Value);
                if (precioMax.HasValue)
                    q = q.Where(p => p.pro_precio <= precioMax.Value);
                if (stockMin.HasValue)
                    q = q.Where(p => p.pro_cantidad >= stockMin.Value);
                if (stockMax.HasValue)
                    q = q.Where(p => p.pro_cantidad <= stockMax.Value);

                var resultados = q
                    .OrderByDescending(p => p.pro_id)
                    .Select(p => new
                    {
                        p.pro_id,
                        p.pro_nombre,
                        p.pro_cantidad,
                        p.pro_precio,
                        p.pro_estado,
                        p.prov_id,
                        prov_nombre = p.tbl_proveedor != null ? p.tbl_proveedor.prov_nombre : null,
                        fotos = p.tbl_pro_fotos
                                  .Select(f => new { f.foto_id, f.foto_ruta, f.fecha_subida, f.foto_estado })
                                  .Take(5).ToList()
                    }).ToList();

                return resultados.Select(p =>
                {
                    var prod = new tbl_producto
                    {
                        pro_id = p.pro_id,
                        pro_nombre = p.pro_nombre,
                        pro_cantidad = p.pro_cantidad,
                        pro_precio = p.pro_precio,
                        pro_estado = p.pro_estado,
                        prov_id = p.prov_id,
                        tbl_proveedor = new tbl_proveedor { prov_nombre = string.IsNullOrWhiteSpace(p.prov_nombre) ? "Sin proveedor" : p.prov_nombre }
                    };
                    var set = new EntitySet<tbl_pro_fotos>();
                    set.AddRange(p.fotos.Select(f => new tbl_pro_fotos
                    {
                        foto_id = f.foto_id,
                        foto_ruta = f.foto_ruta,
                        fecha_subida = f.fecha_subida,
                        foto_estado = f.foto_estado
                    }));
                    prod.tbl_pro_fotos = set;
                    return prod;
                }).ToList();
            }
        }
        // ── Paginado ────────────────────────────────────────────────
        public static List<tbl_producto> BuscarPaginado(
            int pagina, int porPagina, out int totalRegistros,
            string nombre = null, int? proveedorId = null,
            char? estado = null, decimal? precioMin = null, decimal? precioMax = null, int? stockMin = null, int? stockMax = null)
        {
            var todos = Buscar(nombre, proveedorId, estado, precioMin, precioMax, stockMin, stockMax);
            totalRegistros = todos.Count;
            return todos.Skip((pagina - 1) * porPagina).Take(porPagina).ToList();
        }

        public static bool ExisteNombre(string nombre, int idIgnorar = 0)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_producto>()
                    .Any(p => p.pro_nombre == nombre && p.pro_estado == 'A' && p.pro_id != idIgnorar);
            }
        }

        public static List<ProductoCargaFila> LeerArchivoCargaMasiva(byte[] contenidoArchivo, string nombreArchivo)
        {
            return ProductoCargaMasivaParser.Leer(contenidoArchivo, nombreArchivo);
        }

        public static ResultadoCargaProductos ProcesarCargaMasiva(IEnumerable<ProductoCargaFila> filas, TipoInsercionProveedor tipoInsercion)
        {
            var filasNormalizadas = NormalizarFilasCarga(filas);
            var resultado = new ResultadoCargaProductos
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

        // ── Crear ───────────────────────────────────────────────────
        public static void Guardar(tbl_producto producto)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    producto.prov_id = ValidarProveedorExistente(dc, producto.prov_id);
                    producto.pro_estado = 'A';
                    dc.GetTable<tbl_producto>().InsertOnSubmit(producto);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex) { throw new Exception("Error al guardar el producto: " + ex.Message); }
        }

        // ── Modificar ───────────────────────────────────────────────
        public static void Modificar(tbl_producto producto)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var e = dc.GetTable<tbl_producto>()
                        .FirstOrDefault(p => p.pro_id == producto.pro_id)
                        ?? throw new Exception("Producto no encontrado.");
                    e.pro_nombre = producto.pro_nombre;
                    e.pro_cantidad = producto.pro_cantidad;
                    e.pro_precio = producto.pro_precio;
                    e.prov_id = ValidarProveedorExistente(dc, producto.prov_id);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex) { throw new Exception("Error al modificar el producto: " + ex.Message); }
        }

        // ── Activar ─────────────────────────────────────────────────
        public static void Activar(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var prod = dc.GetTable<tbl_producto>()
                        .FirstOrDefault(p => p.pro_id == id)
                        ?? throw new Exception("Producto no encontrado.");
                    prod.pro_estado = 'A';
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex) { throw new Exception("Error al activar el producto: " + ex.Message); }
        }

        // ── Desactivar (lógico) ─────────────────────────────────────
        public static void EliminarLogico(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var prod = dc.GetTable<tbl_producto>()
                        .FirstOrDefault(p => p.pro_id == id)
                        ?? throw new Exception("Producto no encontrado.");
                    prod.pro_estado = 'I';
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex) { throw new Exception("Error al desactivar el producto: " + ex.Message); }
        }

        // ── Eliminación física ──────────────────────────────────────
        public static List<string> EliminarFisico(int id)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var prod = dc.GetTable<tbl_producto>()
                        .FirstOrDefault(p => p.pro_id == id)
                        ?? throw new Exception("Producto no encontrado.");

                    var fotos = dc.GetTable<tbl_pro_fotos>()
                        .Where(f => f.pro_id == id)
                        .ToList();

                    var rutas = fotos
                        .Select(f => f.foto_ruta)
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Distinct()
                        .ToList();

                    if (fotos.Any())
                    {
                        dc.GetTable<tbl_pro_fotos>().DeleteAllOnSubmit(fotos);
                    }

                    dc.GetTable<tbl_producto>().DeleteOnSubmit(prod);
                    dc.SubmitChanges();
                    return rutas;
                }
            }
            catch (Exception ex) { throw new Exception("Error al eliminar el producto: " + ex.Message); }
        }

        private static List<ProductoCargaFilaNormalizada> NormalizarFilasCarga(IEnumerable<ProductoCargaFila> filas)
        {
            var normalizadas = (filas ?? Enumerable.Empty<ProductoCargaFila>())
                .Select(f => new ProductoCargaFilaNormalizada
                {
                    NumeroFilaArchivo = f.NumeroFilaArchivo,
                    ProductoId = f.ProductoId,
                    NombreProducto = (f.NombreProducto ?? string.Empty).Trim(),
                    NombreNormalizado = NormalizarNombreProductoCarga(f.NombreProducto),
                    Cantidad = f.Cantidad,
                    Precio = f.Precio,
                    ProveedorId = f.ProveedorId,
                    EstadoProducto = f.EstadoProducto == 'I' ? 'I' : 'A'
                })
                .ToList();

            if (!normalizadas.Any())
            {
                throw new Exception("No hay filas preparadas para importar.");
            }

            var nombresDuplicados = normalizadas.GroupBy(f => f.NombreNormalizado).FirstOrDefault(g => g.Count() > 1);
            if (nombresDuplicados != null)
            {
                throw new Exception($"El archivo tiene nombres repetidos. Revisa el producto '{nombresDuplicados.First().NombreProducto}'.");
            }

            var idsDuplicados = normalizadas.Where(f => f.ProductoId.HasValue).GroupBy(f => f.ProductoId.Value).FirstOrDefault(g => g.Count() > 1);
            if (idsDuplicados != null)
            {
                throw new Exception($"El archivo tiene IDs repetidos. Revisa el producto con ID {idsDuplicados.Key}.");
            }

            return normalizadas;
        }

        private static void EjecutarCargaIncremental(MonolitoDataContext dc, List<ProductoCargaFilaNormalizada> filas, ResultadoCargaProductos resultado)
        {
            var productosActuales = dc.GetTable<tbl_producto>().ToList();
            var productosPorId = productosActuales.ToDictionary(p => p.pro_id);
            var productosPorNombre = productosActuales.GroupBy(p => NormalizarNombreProductoCarga(p.pro_nombre)).ToDictionary(g => g.Key, g => g.First());
            var proveedoresValidos = dc.GetTable<tbl_proveedor>().Where(p => p.prov_estado == 'A').Select(p => p.prov_id).ToHashSet();
            var insertarConId = new List<ProductoCargaFilaNormalizada>();
            var insertarSinId = new List<ProductoCargaFilaNormalizada>();

            foreach (var fila in filas)
            {
                tbl_producto existente = null;
                if (fila.ProductoId.HasValue) productosPorId.TryGetValue(fila.ProductoId.Value, out existente);
                if (existente == null) productosPorNombre.TryGetValue(fila.NombreNormalizado, out existente);

                int? proveedorId = proveedoresValidos.Contains(fila.ProveedorId ?? 0) ? fila.ProveedorId : (int?)null;
                if (!proveedorId.HasValue && fila.ProveedorId.HasValue) resultado.ProductosSinProveedor++;

                if (existente != null)
                {
                    existente.pro_nombre = fila.NombreProducto;
                    existente.pro_cantidad = fila.Cantidad;
                    existente.pro_precio = fila.Precio;
                    existente.pro_estado = fila.EstadoProducto;
                    existente.prov_id = proveedorId;
                    resultado.Actualizados++;
                    continue;
                }

                fila.ProveedorId = proveedorId;
                if (fila.ProductoId.HasValue) insertarConId.Add(fila);
                else insertarSinId.Add(fila);
            }

            dc.SubmitChanges();
            InsertarProductosConId(dc, insertarConId);
            resultado.Insertados += insertarConId.Count;

            if (insertarSinId.Any())
            {
                var nuevos = insertarSinId.Select(f => new tbl_producto
                {
                    pro_nombre = f.NombreProducto,
                    pro_cantidad = f.Cantidad,
                    pro_precio = f.Precio,
                    pro_estado = f.EstadoProducto,
                    prov_id = f.ProveedorId
                }).ToList();

                dc.GetTable<tbl_producto>().InsertAllOnSubmit(nuevos);
                dc.SubmitChanges();
                resultado.Insertados += nuevos.Count;
            }
        }

        private static void EjecutarReemplazoTotal(MonolitoDataContext dc, List<ProductoCargaFilaNormalizada> filas, ResultadoCargaProductos resultado)
        {
            var fotosRespaldadas = dc.GetTable<tbl_pro_fotos>()
                .Select(f => new
                {
                    f.pro_id,
                    f.foto_bit,
                    f.foto_ruta,
                    f.fecha_subida,
                    f.foto_estado
                })
                .ToList();

            var fotos = dc.GetTable<tbl_pro_fotos>().ToList();
            resultado.FotosEliminadas = fotos.Count;
            if (fotos.Any())
            {
                dc.GetTable<tbl_pro_fotos>().DeleteAllOnSubmit(fotos);
                dc.SubmitChanges();
            }

            var productos = dc.GetTable<tbl_producto>().ToList();
            if (productos.Any())
            {
                dc.GetTable<tbl_producto>().DeleteAllOnSubmit(productos);
                dc.SubmitChanges();
            }

            dc.ExecuteCommand("DBCC CHECKIDENT ('dbo.tbl_pro_fotos', RESEED, 0)");
            dc.ExecuteCommand("DBCC CHECKIDENT ('dbo.tbl_producto', RESEED, 0)");

            var proveedoresValidos = dc.GetTable<tbl_proveedor>().Where(p => p.prov_estado == 'A').Select(p => p.prov_id).ToHashSet();
            foreach (var fila in filas)
            {
                int? proveedorId = proveedoresValidos.Contains(fila.ProveedorId ?? 0) ? fila.ProveedorId : (int?)null;
                if (!proveedorId.HasValue && fila.ProveedorId.HasValue) resultado.ProductosSinProveedor++;

                var producto = new tbl_producto
                {
                    pro_nombre = fila.NombreProducto,
                    pro_cantidad = fila.Cantidad,
                    pro_precio = fila.Precio,
                    pro_estado = fila.EstadoProducto,
                    prov_id = proveedorId
                };

                dc.GetTable<tbl_producto>().InsertOnSubmit(producto);
                dc.SubmitChanges();
                resultado.Insertados++;
            }

            var productosRecargados = dc.GetTable<tbl_producto>()
                .Select(p => p.pro_id)
                .ToHashSet();

            var fotosAReinsertar = fotosRespaldadas
                .Where(f => productosRecargados.Contains(f.pro_id))
                .Select(f => new tbl_pro_fotos
                {
                    pro_id = f.pro_id,
                    foto_bit = f.foto_bit,
                    foto_ruta = f.foto_ruta,
                    fecha_subida = f.fecha_subida,
                    foto_estado = f.foto_estado
                })
                .ToList();

            if (fotosAReinsertar.Any())
            {
                dc.GetTable<tbl_pro_fotos>().InsertAllOnSubmit(fotosAReinsertar);
                dc.SubmitChanges();
            }
        }

        private static void InsertarProductosConId(MonolitoDataContext dc, IEnumerable<ProductoCargaFilaNormalizada> filas)
        {
            var lista = (filas ?? Enumerable.Empty<ProductoCargaFilaNormalizada>()).ToList();
            if (!lista.Any()) return;

            dc.ExecuteCommand("SET IDENTITY_INSERT dbo.tbl_producto ON");
            try
            {
                foreach (var fila in lista)
                {
                    dc.ExecuteCommand(
                        "INSERT INTO dbo.tbl_producto (pro_id, pro_nombre, pro_cantidad, pro_precio, pro_estado, prov_id) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        fila.ProductoId.Value, fila.NombreProducto, fila.Cantidad, fila.Precio, fila.EstadoProducto, fila.ProveedorId);
                }
            }
            finally
            {
                dc.ExecuteCommand("SET IDENTITY_INSERT dbo.tbl_producto OFF");
            }
        }

        private static int? ValidarProveedorExistente(MonolitoDataContext dc, int? proveedorId)
        {
            if (!proveedorId.HasValue) return null;
            return dc.GetTable<tbl_proveedor>().Any(p => p.prov_id == proveedorId.Value && p.prov_estado == 'A')
                ? proveedorId
                : (int?)null;
        }

        private static string NormalizarNombreProductoCarga(string nombre)
        {
            return (nombre ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
