using Capa_Negocios;
using System;
using System.Web;

namespace Monolito4bm
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.Cookies["UsuarioRecordado"] != null)
                {
                    txtNick.Text = Request.Cookies["UsuarioRecordado"].Value;
                    chkRecordar.Checked = true;
                }

                if (Request.Cookies["PassRecordada"] != null)
                {
                    string passGuardada = Request.Cookies["PassRecordada"].Value;
                    string script = $"document.getElementById('txtPass').value = '{passGuardada.Replace("'", "\\'")}';";
                    ClientScript.RegisterStartupScript(this.GetType(), "SetPass", script, true);
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string nick = txtNick.Text.Trim();
            string pass = txtPass.Text;

            int rolId;
            string correo, msgError;

            ResultadoLogin resultado = _svc.IniciarSesion(nick, pass, out rolId, out correo, out msgError);

            switch (resultado)
            {
                case ResultadoLogin.Exitoso:
                    Session["correo_qr"] = correo;
                    Session["rol_id"] = rolId;

                    if (chkRecordar.Checked)
                    {
                        HttpCookie cookieNick = new HttpCookie("UsuarioRecordado", txtNick.Text.Trim());
                        cookieNick.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(cookieNick);

                        HttpCookie cookiePass = new HttpCookie("PassRecordada", txtPass.Text);
                        cookiePass.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(cookiePass);
                    }
                    else
                    {
                        if (Request.Cookies["UsuarioRecordado"] != null)
                        {
                            HttpCookie cookie = new HttpCookie("UsuarioRecordado");
                            cookie.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Add(cookie);
                        }
                        if (Request.Cookies["PassRecordada"] != null)
                        {
                            HttpCookie cookiePass = new HttpCookie("PassRecordada");
                            cookiePass.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Add(cookiePass);
                        }
                    }
                    Response.Redirect("~/VerificarQR.aspx");
                    break;

                case ResultadoLogin.ExitosoClaveTemporalActiva:
                    // AQUÍ ESTÁ LA MAGIA: Llamamos a la nueva función de la Capa de Negocios
                    Session["usu_id"] = _svc.ObtenerIdPorNick(nick);
                    Session["clave_temporal"] = true;
                    Response.Redirect("~/CambiarClave.aspx");
                    break;

                case ResultadoLogin.UsuarioBloqueado:
                    InjectarSweetAlert("error", "Cuenta Bloqueada", msgError);
                    txtPass.Text = string.Empty;
                    break;

                case ResultadoLogin.CredencialesInvalidas:
                    InjectarSweetAlert("warning", "Datos incorrectos", msgError);
                    txtPass.Text = string.Empty;
                    break;

                case ResultadoLogin.ErrorInterno:
                    InjectarSweetAlert("error", "Error del Sistema", msgError);
                    txtPass.Text = string.Empty;
                    break;
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect("register.aspx");
        }

        protected void btnRecuperar_Click(object sender, EventArgs e)
        {
            Response.Redirect("RecuperarClave.aspx");
        }

        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire('{titulo}', '{msjLimpio}', '{tipo}');";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalError", script, true);
        }
    }
}