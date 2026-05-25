using Capa_Datos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios
{
    public class CN_tbl_pro_fotos
    {
        public static List<tbl_pro_fotos> ObtenerPorProducto(int proId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.tbl_pro_fotos
                         .Where(f => f.pro_id == proId)
                         .OrderByDescending(f => f.fecha_subida)
                         .ToList();
            }
        }

        public static string EliminarFisico(int fotoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                var foto = dc.tbl_pro_fotos.FirstOrDefault(f => f.foto_id == fotoId);
                if (foto == null) throw new Exception("Foto no encontrada.");

                string ruta = foto.foto_ruta;
                dc.tbl_pro_fotos.DeleteOnSubmit(foto);
                dc.SubmitChanges();
                return ruta;
            }
        }

        public static List<tbl_pro_fotos> ObtenerConProducto(int productoId)
        {
            using (var dc = new MonolitoDataContext())
            {
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
                    .ToList();

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
        }

        public static List<tbl_pro_fotos> ObtenerPorProductoiN(int productoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_pro_fotos>()
                         .Where(f => f.pro_id == productoId)
                         .OrderByDescending(f => f.fecha_subida)
                         .ToList();
            }
        }

        public static int Contar(int productoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_pro_fotos>()
                         .Count(f => f.pro_id == productoId);
            }
        }

        public static tbl_pro_fotos BuscarPorId(int fotoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_pro_fotos>()
                         .FirstOrDefault(f => f.foto_id == fotoId);
            }
        }

        public static void GuardarFotos(List<tbl_pro_fotos> fotos)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    dc.GetTable<tbl_pro_fotos>().InsertAllOnSubmit(fotos);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar fotos: " + ex.Message);
            }
        }

        public static void Guardar(tbl_pro_fotos foto)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    foto.fecha_subida = DateTime.Now;
                    foto.foto_estado = 'A';
                    dc.GetTable<tbl_pro_fotos>().InsertOnSubmit(foto);
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar foto: " + ex.Message);
            }
        }

        public static void CambiarEstado(int fotoId, char nuevoEstado)
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var foto = dc.GetTable<tbl_pro_fotos>()
                        .FirstOrDefault(f => f.foto_id == fotoId)
                        ?? throw new Exception("Foto no encontrada.");
                    foto.foto_estado = nuevoEstado;
                    dc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar estado de foto: " + ex.Message);
            }
        }
    }
}
