using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class RecuperarClave : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            string valor = txtNickOCorreo.Text.Trim();
            string mensajeErr;

            ResultadoRecuperacion resultado = _svc.RecuperarClave(valor, out mensajeErr);

            switch (resultado)
            {
                case ResultadoRecuperacion.Exitoso:
                    // Enviamos mensaje de éxito y al cerrar la alerta redirige al Login
                    string msjExito = $"{mensajeErr}<br><br><b>Ingrese con su clave temporal.</b>";
                    InjectarSweetAlertConRedireccion("success", "Clave Enviada", msjExito);

                    txtNickOCorreo.Text = string.Empty;
                    btnEnviar.Enabled = false;
                    break;

                case ResultadoRecuperacion.UsuarioNoEncontrado:
                    // Alerta amarilla si no se encuentra
                    InjectarSweetAlert("warning", "Usuario no encontrado", mensajeErr);
                    break;

                case ResultadoRecuperacion.ErrorInterno:
                    // Alerta roja si ocurre un error de envío de correo o base de datos
                    InjectarSweetAlert("error", "Error Interno", mensajeErr);
                    break;
            }
        }

        // Método para Alertas Simples
        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire('{titulo}', '{msjLimpio}', '{tipo}');";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalError", script, true);
        }

        // Método para Alertas que redirigen
        private void InjectarSweetAlertConRedireccion(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire({{ title: '{titulo}', html: '{msjLimpio}', icon: '{tipo}' }}).then(function() {{ window.location = 'Default.aspx'; }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalRedirect", script, true);
        }
    }
}