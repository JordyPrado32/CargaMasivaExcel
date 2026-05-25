using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios
{
    public class tbl_tipo_usuario
    {
        public int rol_id { get; set; }
        public string nombre_rol { get; set; }

        public List<tbl_tipo_usuario> CargarRoles()
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                return dc.tbl_rol
                    .Select(r => new tbl_tipo_usuario
                    {
                        rol_id = r.rol_id,
                        nombre_rol = r.nombre_rol
                    })
                    .ToList();
            }
        }
    }
}
