using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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

        private string cadena = "Data Source=.;Initial Catalog=deberes_4to;User ID=clase4b;Password=clase4b;Encrypt=False;";

        public void RegistrarFotosValidadas(int usuarioId, List<tbl_foto> listaFotos)
        {
            if (listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe subir entre 3 y 5 fotografías válidas.");

            using (SqlConnection con = new SqlConnection(cadena))
            {
                con.Open();
                bool primera = true;
                foreach (var f in listaFotos)
                {
                    string query = @"INSERT INTO tbl_usuario_fotos 
                                    (usu_id, nombre_archivo, content_type, foto, fecha_subida, es_principal) 
                                    VALUES (@uid, @nom, @tip, @fot, @fec, @pri)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        cmd.Parameters.AddWithValue("@nom", f.nombre_archivo);
                        cmd.Parameters.AddWithValue("@tip", f.content_type);
                        cmd.Parameters.AddWithValue("@fot", f.foto);
                        cmd.Parameters.AddWithValue("@fec", DateTime.Now);
                        cmd.Parameters.AddWithValue("@pri", primera);
                        cmd.ExecuteNonQuery();
                    }
                    primera = false;
                }
            }
        }
    }
}