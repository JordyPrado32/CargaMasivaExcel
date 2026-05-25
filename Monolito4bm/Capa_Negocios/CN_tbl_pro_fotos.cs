using Capa_Datos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios
{
    public class CN_tbl_pro_fotos
    {
        private static MonolitoDataContext dc = new MonolitoDataContext();

        // ── Obtener todas las fotos de un producto ────────────────
        public static List<tbl_pro_fotos> ObtenerPorProducto(int proId)
        {
            return dc.tbl_pro_fotos
                     .Where(f => f.pro_id == proId)
                     .OrderByDescending(f => f.fecha_subida)
                     .ToList();
        }
        // ── Eliminar físico por id ────────────────────────────────
        public static string EliminarFisico(int fotoId)
        {
            var foto = dc.tbl_pro_fotos.FirstOrDefault(f => f.foto_id == fotoId);
            if (foto == null) throw new Exception("Foto no encontrada.");

            string ruta = foto.foto_ruta; // devolvemos la ruta para borrar el archivo
            dc.tbl_pro_fotos.DeleteOnSubmit(foto);
            dc.SubmitChanges();
            return ruta;
        }

        public static List<tbl_pro_fotos> ObtenerConProducto(int productoId)
        {
            // PASO 1: Traemos los datos crudos de la base de datos usando un tipo anónimo
            var datosCrudos = dc.GetTable<tbl_pro_fotos>()
                     .Where(f => f.pro_id == productoId)
                     .Select(f => new
                     {
                         f.foto_id,
                         f.pro_id,
                         f.foto_ruta,
                         f.foto_estado,
                         f.fecha_subida,
                         pro_nombre = f.tbl_producto.pro_nombre
                     })
                     .OrderByDescending(f => f.fecha_subida)
                     .ToList(); // Al poner ToList() aquí, cerramos la conexión con la BD

            // PASO 2: Ahora que los datos están en memoria, armamos nuestras clases reales
            return datosCrudos.Select(x => new tbl_pro_fotos
            {
                foto_id = x.foto_id,
                pro_id = x.pro_id,
                foto_ruta = x.foto_ruta,
                foto_estado = x.foto_estado,
                fecha_subida = x.fecha_subida,
                tbl_producto = new tbl_producto
                {
                    pro_nombre = x.pro_nombre
                }
            }).ToList();
        }
        // ── Fotos de un producto sin relaciones (uso interno) ───────
        public static List<tbl_pro_fotos> ObtenerPorProductoiN(int productoId)
        {
            return dc.GetTable<tbl_pro_fotos>()
                     .Where(f => f.pro_id == productoId)
                     .OrderByDescending(f => f.fecha_subida)
                     .ToList();
        }

        // ── Contar fotos de un producto ─────────────────────────────
        public static int Contar(int productoId)
        {
            return dc.GetTable<tbl_pro_fotos>()
                     .Count(f => f.pro_id == productoId);
        }

        // ── Buscar por ID (objeto rastreado) ────────────────────────
        public static tbl_pro_fotos BuscarPorId(int fotoId)
        {
            return dc.GetTable<tbl_pro_fotos>()
                     .FirstOrDefault(f => f.foto_id == fotoId);
        }

        // ── Guardar una o varias fotos nuevas ───────────────────────
        public static void GuardarFotos(List<tbl_pro_fotos> fotos)
        {
            try
            {
                dc.GetTable<tbl_pro_fotos>().InsertAllOnSubmit(fotos);
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar fotos: " + ex.Message);
            }
        }

        // ── Guardar una sola foto ───────────────────────────────────
        public static void Guardar(tbl_pro_fotos foto)
        {
            try
            {
                foto.fecha_subida = DateTime.Now;
                foto.foto_estado = 'A';
                dc.GetTable<tbl_pro_fotos>().InsertOnSubmit(foto);
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar foto: " + ex.Message);
            }
        }

        // ── Cambiar estado  A ↔ I  (desactivar / reactivar) ────────
        public static void CambiarEstado(int fotoId, char nuevoEstado)
        {
            try
            {
                var foto = BuscarPorId(fotoId)
                    ?? throw new Exception("Foto no encontrada.");
                foto.foto_estado = nuevoEstado;
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar estado de foto: " + ex.Message);
            }
        }

    }
}