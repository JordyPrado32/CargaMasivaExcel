using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class Site1 : MasterPage
    {
        protected global::System.Web.UI.WebControls.Literal litNombre;
        protected global::System.Web.UI.WebControls.Literal litavatar;
        protected global::System.Web.UI.WebControls.LinkButton btnSalir;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarNombre();
                CargarAvatar();
            }
        }

        private void CargarNombre()
        {
            // Con las sesiones corregidas ya tenemos nombre_usuario
            // Pero por si acaso también busca por correo_qr como fallback
            string nombre = Session["nombre_usuario"]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                // Fallback: busca el nombre usando el correo guardado en sesión
                string correo = Session["correo_qr"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    try
                    {
                        var svc = new UsuarioService();
                        int id = svc.ObtenerIdPorNick(correo);
                        if (id > 0)
                        {
                            var datos = svc.ObtenerDatosUsuario(id);
                            nombre = datos?.usu_nombres ?? string.Empty;
                            // Guardamos en sesión para no volver a buscar
                            Session["usu_id"] = id;
                            Session["nombre_usuario"] = nombre;
                        }
                    }
                    catch { }
                }
            }

            litNombre.Text = string.IsNullOrWhiteSpace(nombre)
                ? "Usuario"
                : nombre.Trim()
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() ?? "Usuario";
        }

        private void CargarAvatar()
        {
            // usu_id puede ya estar en sesión o haberlo puesto CargarNombre()
            if (Session["usu_id"] == null)
            {
                LogAvatar("Session usu_id vacio");
                litavatar.Text = Placeholder();
                return;
            }

            int usuId = Convert.ToInt32(Session["usu_id"]);

            try
            {
                var fotos = CN_tbl_foto.ObtenerFotosPorUsuario(usuId);
                LogAvatar($"Usuario={usuId} fotos={fotos.Count}");

                // LINQ: primero la principal, si no cualquiera
                var foto = fotos.FirstOrDefault(f => f.es_principal)
                        ?? fotos.FirstOrDefault();

                if (foto != null)
                {
                    int longitud = foto.foto != null ? foto.foto.Length : 0;
                    LogAvatar($"Foto seleccionada id={foto.foto_id} principal={foto.es_principal} bytes={longitud} ruta={foto.foto_ruta ?? "(null)"} contentType={foto.content_type ?? "(null)"}");
                    string src;
                    if (foto.foto != null && foto.foto.Length > 0)
                    {
                        string mime = string.IsNullOrWhiteSpace(foto.content_type) ? "image/jpeg" : foto.content_type;
                        string base64 = Convert.ToBase64String(foto.foto.ToArray());
                        src = "data:" + mime + ";base64," + base64;
                        LogAvatar($"Render inline base64 id={foto.foto_id} bytes={base64.Length}");
                    }
                    else
                    {
                        src = ResolveUrl("~/ImagenUsuarioFallback.ashx?id=" + foto.foto_id);
                        LogAvatar($"Render por handler id={foto.foto_id} src={src}");
                    }

                    litavatar.Text = $"<img src='{src}' " +
                                     "alt='Foto de perfil' " +
                                     "style='width:100%;height:100%;object-fit:cover;border-radius:50%;'/>";
                }
                else
                {
                    LogAvatar($"Usuario={usuId} sin foto seleccionada");
                    litavatar.Text = Placeholder();
                }
            }
            catch (Exception ex)
            {
                LogAvatar($"Error avatar usuario={usuId}: {ex}");
                litavatar.Text = Placeholder();
            }
        }

        private string Placeholder() =>
            "<i class='fa-solid fa-user' style='font-size:1.4rem;color:#7a4aaa;'></i>";

        private void LogAvatar(string mensaje)
        {
            try
            {
                string carpeta = Server.MapPath("~/Uploads");
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                string archivo = Path.Combine(carpeta, "avatar-debug.log");
                string linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MASTER {mensaje}{Environment.NewLine}";
                File.AppendAllText(archivo, linea);
            }
            catch
            {
            }
        }

        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Session.Keys
                   .Cast<string>()
                   .ToList()
                   .ForEach(k => Session.Remove(k));

            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Default.aspx");
        }
    }
}
