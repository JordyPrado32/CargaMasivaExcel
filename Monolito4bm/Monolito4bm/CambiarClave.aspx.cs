using System;
using System.Text.RegularExpressions;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class CambiarClave : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["usu_id"] == null || Session["clave_temporal"] == null
                || !(bool)Session["clave_temporal"])
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void btnCambiar_Click(object sender, EventArgs e)
        {
            string nueva = txtNueva.Text;
            string confirmar = txtConfirmar.Text;

            if (nueva != confirmar)
            {
                InjectarSweetAlert("error", "Error", "Las contraseñas no coinciden.");
                return;
            }

            // Validar en el Backend (C#)
            string patron = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,16}$";
            if (!Regex.IsMatch(nueva, patron))
            {
                InjectarSweetAlert("warning", "Seguridad Débil", "La contraseña no cumple con los requisitos de seguridad. Revisa las reglas en pantalla.");
                return;
            }

            try
            {
                int usuId = Convert.ToInt32(Session["usu_id"]);
                _svc.CambiarContrasena(usuId, nueva);

                Session["clave_temporal"] = false;

                // Si todo va bien, mostramos la alerta de éxito y lo mandamos de vuelta al Login
                InjectarSweetAlertConRedireccion("success", "¡Cambio Exitoso!", "Tu contraseña ha sido actualizada correctamente. Por favor, inicia sesión.");
            }
            catch (ArgumentException ex)
            {
                InjectarSweetAlert("error", "Atención", ex.Message);
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("error", "Error interno", ex.Message);
            }
        }

        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire('{titulo}', '{msjLimpio}', '{tipo}');";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalError", script, true);
        }

        private void InjectarSweetAlertConRedireccion(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire({{ title: '{titulo}', text: '{msjLimpio}', icon: '{tipo}' }}).then(function() {{ window.location = 'Default.aspx'; }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalRedirect", script, true);
        }
    }
}