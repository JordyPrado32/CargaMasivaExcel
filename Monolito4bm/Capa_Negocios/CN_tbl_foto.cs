using Capa_Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using tbl_usuario_foto = Capa_Datos.tbl_usuario_fotos;

namespace Capa_Negocios
{
    public class CN_tbl_foto
    {
        public static List<tbl_usuario_foto> ObtenerFotosPorUsuario(int usuId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_usuario_foto>()
                    .Where(f => f.usu_id == usuId)
                    .OrderByDescending(f => f.es_principal)
                    .ThenByDescending(f => f.fecha_subida)
                    .ToList();
            }
        }

        public static void RegistrarFotos(int usuarioId, List<tbl_usuario_foto> listaFotos)
        {
            if (listaFotos == null || listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe subir entre 3 y 5 fotografias validas.");

            if (!listaFotos.All(f => f.foto != null && f.foto.Length > 0))
                throw new Exception("Una o mas fotografias estan vacias.");

            listaFotos
                .Select((f, i) => new { f, i })
                .ToList()
                .ForEach(x =>
                {
                    x.f.usu_id = usuarioId;
                    x.f.es_principal = x.i == 0;
                    x.f.fecha_subida = DateTime.Now;
                });

            using (var dc = new MonolitoDataContext())
            {
                dc.tbl_usuario_fotos.InsertAllOnSubmit(listaFotos);
                dc.SubmitChanges();
            }
        }

        public static tbl_usuario_foto ObtenerFallback(int fotoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_usuario_foto>()
                    .Where(f => f.foto_id == fotoId && f.foto != null)
                    .FirstOrDefault();
            }
        }

        public static tbl_usuario_foto ObtenerParaResolver(int fotoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_usuario_foto>()
                    .Where(f => f.foto_id == fotoId)
                    .FirstOrDefault();
            }
        }

    }
}
