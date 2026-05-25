using System;
using System.Collections.Generic;

namespace Capa_Negocios
{
    public class tbl_foto
    {
        public int foto_id { get; set; }
        public int usu_id { get; set; }
        public string nombre_archivo { get; set; }
        public string content_type { get; set; }
        public byte[] foto { get; set; }
        public DateTime fecha_subida { get; set; }
        public bool es_principal { get; set; }

        public void RegistrarFotosValidadas(int usuarioId, List<tbl_foto> listaFotos)
        {
            if (listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe subir entre 3 y 5 fotografías válidas.");

            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                bool primera = true;
                foreach (var f in listaFotos)
                {
                    dc.tbl_usuario_fotos.InsertOnSubmit(new Capa_Datos.tbl_usuario_fotos
                    {
                        usu_id = usuarioId,
                        nombre_archivo = f.nombre_archivo,
                        content_type = f.content_type,
                        foto = f.foto,
                        fecha_subida = DateTime.Now,
                        es_principal = primera
                    });
                    primera = false;
                }

                dc.SubmitChanges();
            }
        }
    }
}
