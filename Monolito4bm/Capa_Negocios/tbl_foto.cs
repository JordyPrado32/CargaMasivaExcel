using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Capa_Datos;
using tbl_usuario_foto = Capa_Datos.tbl_usuario_fotos;

namespace Capa_Negocios
{
    public class tbl_foto
    {
        public int foto_id { get; set; }
        public int usu_id { get; set; }
        public string nombre_archivo { get; set; }
        public string content_type { get; set; }
        public byte[] foto { get; set; }
        public string foto_ruta { get; set; }
        public DateTime fecha_subida { get; set; }
        public bool es_principal { get; set; }

        public void RegistrarFotosValidadas(int usuarioId, List<tbl_foto> listaFotos)
        {
            if (listaFotos == null || listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe subir entre 3 y 5 fotografias validas.");

            using (var dc = new MonolitoDataContext())
            {
                var fechaBase = DateTime.Now;
                var entidades = listaFotos
                    .Select((f, index) => new tbl_usuario_foto
                    {
                        usu_id = usuarioId,
                        nombre_archivo = f.nombre_archivo,
                        content_type = f.content_type,
                        foto = f.foto != null ? new Binary(f.foto) : null,
                        fecha_subida = fechaBase,
                        es_principal = index == 0,
                        foto_ruta = f.foto_ruta
                    })
                    .ToList();

                dc.tbl_usuario_fotos.InsertAllOnSubmit(entidades);
                dc.SubmitChanges();
            }
        }
    }
}
