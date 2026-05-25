using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    public class CN_tbl_producto
    {
        private static MonolitoDataContext dc = new MonolitoDataContext();

        // ── Proyección interna con proveedor + fotos ─────────────────
        // ── Proyección interna con proveedor + fotos ─────────────────
        private static IEnumerable<tbl_producto> QueryConRelaciones()
        {
            var datosBD = dc.GetTable<tbl_producto>()
                .OrderByDescending(p => p.pro_id)   // <-- más reciente primero
                .Select(p => new
                {
                    p.pro_id,
                    p.pro_nombre,
                    p.pro_cantidad,
                    p.pro_precio,
                    p.pro_estado,
                    p.prov_id,
                    prov_nombre = p.tbl_proveedor.prov_nombre,
                    fotos = p.tbl_pro_fotos
                              .Select(f => new { f.foto_id, f.pro_id, f.foto_ruta, f.fecha_subida, f.foto_estado }) // <-- AGREGADO
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
                    tbl_proveedor = new tbl_proveedor { prov_nombre = p.prov_nombre }
                };
                var set = new EntitySet<tbl_pro_fotos>();
                set.AddRange(p.fotos.Select(f => new tbl_pro_fotos
                {
                    foto_id = f.foto_id,
                    pro_id = f.pro_id,
                    foto_ruta = f.foto_ruta,
                    fecha_subida = f.fecha_subida,
                    foto_estado = f.foto_estado // <-- AGREGADO
                }));
                prod.tbl_pro_fotos = set;
                return prod;
            });
        }

        public static List<tbl_producto> Listar()
            => QueryConRelaciones().ToList();

        public static tbl_producto BuscarPorId(int id)
            => dc.GetTable<tbl_producto>().FirstOrDefault(p => p.pro_id == id);

        // ── Búsqueda avanzada ───────────────────────────────────────
        // ── Búsqueda avanzada ───────────────────────────────────────
        public static List<tbl_producto> Buscar(
            string nombre = null,
            int? proveedorId = null,
            char? estado = null,
            decimal? precioMin = null,
            decimal? precioMax = null)
        {
            var q = dc.GetTable<tbl_producto>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                q = q.Where(p => p.pro_nombre.Contains(nombre));
            if (proveedorId.HasValue)
                q = q.Where(p => p.prov_id == proveedorId.Value);
            if (estado.HasValue)
                q = q.Where(p => p.pro_estado == estado.Value);
            if (precioMin.HasValue)
                q = q.Where(p => p.pro_precio >= precioMin.Value);
            if (precioMax.HasValue)
                q = q.Where(p => p.pro_precio <= precioMax.Value);

            var resultados = q
                .OrderByDescending(p => p.pro_id)   // <-- más reciente primero
                .Select(p => new
                {
                    p.pro_id,
                    p.pro_nombre,
                    p.pro_cantidad,
                    p.pro_precio,
                    p.pro_estado,
                    p.prov_id,
                    prov_nombre = p.tbl_proveedor.prov_nombre,
                    fotos = p.tbl_pro_fotos
                              .Select(f => new { f.foto_id, f.foto_ruta, f.fecha_subida, f.foto_estado }) // <-- AGREGADO
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
                    tbl_proveedor = new tbl_proveedor { prov_nombre = p.prov_nombre }
                };
                var set = new EntitySet<tbl_pro_fotos>();
                set.AddRange(p.fotos.Select(f => new tbl_pro_fotos
                {
                    foto_id = f.foto_id,
                    foto_ruta = f.foto_ruta,
                    fecha_subida = f.fecha_subida,
                    foto_estado = f.foto_estado // <-- AGREGADO
                }));
                prod.tbl_pro_fotos = set;
                return prod;
            }).ToList();
        }
        // ── Paginado ────────────────────────────────────────────────
        public static List<tbl_producto> BuscarPaginado(
            int pagina, int porPagina, out int totalRegistros,
            string nombre = null, int? proveedorId = null,
            char? estado = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            var todos = Buscar(nombre, proveedorId, estado, precioMin, precioMax);
            totalRegistros = todos.Count;
            return todos.Skip((pagina - 1) * porPagina).Take(porPagina).ToList();
        }

        public static bool ExisteNombre(string nombre, int idIgnorar = 0)
            => dc.GetTable<tbl_producto>()
                 .Any(p => p.pro_nombre == nombre && p.pro_estado == 'A' && p.pro_id != idIgnorar);

        // ── Crear ───────────────────────────────────────────────────
        public static void Guardar(tbl_producto producto)
        {
            try
            {
                producto.pro_estado = 'A';
                dc.GetTable<tbl_producto>().InsertOnSubmit(producto);
                dc.SubmitChanges();
            }
            catch (Exception ex) { throw new Exception("Error al guardar el producto: " + ex.Message); }
        }

        // ── Modificar ───────────────────────────────────────────────
        public static void Modificar(tbl_producto producto)
        {
            try
            {
                var e = BuscarPorId(producto.pro_id)
                    ?? throw new Exception("Producto no encontrado.");
                e.pro_nombre = producto.pro_nombre;
                e.pro_cantidad = producto.pro_cantidad;
                e.pro_precio = producto.pro_precio;
                e.prov_id = producto.prov_id;
                dc.SubmitChanges();
            }
            catch (Exception ex) { throw new Exception("Error al modificar el producto: " + ex.Message); }
        }

        // ── Activar ─────────────────────────────────────────────────
        public static void Activar(int id)
        {
            try
            {
                var prod = BuscarPorId(id)
                    ?? throw new Exception("Producto no encontrado.");
                prod.pro_estado = 'A';
                dc.SubmitChanges();
            }
            catch (Exception ex) { throw new Exception("Error al activar el producto: " + ex.Message); }
        }

        // ── Desactivar (lógico) ─────────────────────────────────────
        public static void EliminarLogico(int id)
        {
            try
            {
                var prod = BuscarPorId(id)
                    ?? throw new Exception("Producto no encontrado.");
                prod.pro_estado = 'I';
                dc.SubmitChanges();
            }
            catch (Exception ex) { throw new Exception("Error al desactivar el producto: " + ex.Message); }
        }

        // ── Eliminación física ──────────────────────────────────────
        public static void EliminarFisico(int id)
        {
            try
            {
                var prod = BuscarPorId(id)
                    ?? throw new Exception("Producto no encontrado.");
                dc.GetTable<tbl_producto>().DeleteOnSubmit(prod);
                dc.SubmitChanges();
            }
            catch (Exception ex) { throw new Exception("Error al eliminar el producto: " + ex.Message); }
        }
    }
}