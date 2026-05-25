using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Capa_Negocios
{
    public class tbl_tipo_usuario
    {
        public int rol_id { get; set; }
        public string nombre_rol { get; set; }

        private string cadena = "Data Source=.;Initial Catalog=deberes_4to;User ID=clase4b;Password=clase4b;Encrypt=False;";

        public List<tbl_tipo_usuario> CargarRoles()
        {
            List<tbl_tipo_usuario> lista = new List<tbl_tipo_usuario>();
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT rol_id, nombre_rol FROM tbl_rol";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new tbl_tipo_usuario
                    {
                        rol_id = Convert.ToInt32(row["rol_id"]),
                        nombre_rol = row["nombre_rol"].ToString()
                    });
                }
            }
            return lista;
        }
    }
}