using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class VerificarQR : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Proteger: si no hay sesión activa, redirigir al login
            if (Session["correo_qr"] == null)
                Response.Redirect("~/Default.aspx");
        }

        protected void btnValidar_Click(object sender, EventArgs e)
        {
            string otp = hdnOtp.Value.Trim();

            if (string.IsNullOrEmpty(otp))
                return;

            int rolId = _svc.ValidarOtp(otp);

            switch (rolId)
            {
                case 1:
                    Session["autenticado"] = true;
                    int usuId = _svc.ObtenerIdPorNick(Session["correo_qr"].ToString());
                    var datos = _svc.ObtenerDatosUsuario(usuId);
                    Session["usu_id"] = usuId;
                    Session["nombre_usuario"] = datos.usu_nombres;
                    Session["autenticado"] = true;
                    Response.Redirect("~/Proveedores.aspx");
                    break;

                case 2:
                    Session["autenticado"] = true;
                    Response.Redirect("~/PaginaRol2.aspx");
                    break;

                default:
                    // OTP inválido o expirado — Mostrar SweetAlert y enviar al Login
                    Session.Clear();
                    InjectarSweetAlertConRedireccion("error", "Código Inválido", "El código QR es incorrecto o ha caducado (duraba 5 minutos).");
                    break;
            }
        }

        // Método para inyectar alertas visuales desde el servidor
        private void InjectarSweetAlertConRedireccion(string tipo, string titulo, string mensaje)
        {
            string script = $"Swal.fire('{titulo}', '{mensaje}', '{tipo}').then(function() {{ window.location = 'Default.aspx'; }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalRedirect", script, true);
        }
    }
}