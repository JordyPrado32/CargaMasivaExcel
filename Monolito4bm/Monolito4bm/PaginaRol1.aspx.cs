using System;
using System.Data;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class PaginaRol1 : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!EsAdminAutenticado())
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
                CargarUsuarios();
        }

        protected void btnRecargar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
            InjectarSweetAlert("success", "Actualizado", "El listado de usuarios ha sido actualizado.");
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        protected void gvUsuarios_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int usuId))
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "ResetearIntentos":
                        _svc.ResetearIntentosUsuario(usuId);
                        // Sin tildes en el mensaje
                        InjectarSweetAlert("success", "Intentos Reseteados", "Los intentos del usuario fueron reiniciados a 0 exitosamente.");
                        break;

                    case "DesbloquearCuenta":
                        string correoDestino;
                        string claveTemporal;
                        _svc.DesbloquearCuentaConClaveTemporal(usuId, out correoDestino, out claveTemporal);
                        // Sin tilde en "envio"
                        InjectarSweetAlert("success", "Cuenta Desbloqueada", $"La cuenta fue desbloqueada y se envio una clave temporal al correo: <b>{correoDestino}</b>.");
                        break;
                }
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("error", "Error", ex.Message);
            }

            CargarUsuarios();
        }

        protected bool EsCuentaBloqueada(object estado)
        {
            return Convert.ToString(estado).IndexOf("bloque", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CargarUsuarios()
        {
            DataTable dt = _svc.ObtenerEstadoCuentas();
            gvUsuarios.DataSource = dt;
            gvUsuarios.DataBind();
        }

        private bool EsAdminAutenticado()
        {
            return Session["autenticado"] != null
                   && Convert.ToBoolean(Session["autenticado"])
                   && Session["rol_id"] != null
                   && Convert.ToInt32(Session["rol_id"]) == 1;
        }

        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");
            string script = $"Swal.fire({{ title: '{titulo}', html: '{msjLimpio}', icon: '{tipo}' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SwalServer", script, true);
        }
    }
}