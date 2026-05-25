using Capa_Datos;
using Capa_Negocios;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Monolito4bm
{
    public partial class Proveedores : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Literal litMensaje;
        protected global::System.Web.UI.WebControls.Literal litTituloForm;
        protected global::System.Web.UI.WebControls.HiddenField hfProvId;
        protected global::System.Web.UI.WebControls.TextBox txtNombre;
        protected global::System.Web.UI.WebControls.LinkButton btnGuardar;
        protected global::System.Web.UI.WebControls.Button btnCancelar;
        protected global::System.Web.UI.WebControls.GridView gvProveedores;
        protected global::System.Web.UI.WebControls.TextBox txtBuscar;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;
        protected global::System.Web.UI.WebControls.HiddenField hfModalAbierto;
        protected global::System.Web.UI.WebControls.HiddenField hfFiltrosAbiertos;
        protected global::System.Web.UI.WebControls.LinkButton btnLimpiarFiltros;
        // ── Page_Load ────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarGrid();
        }

        // ── Llenar GridView ──────────────────────────────────────────
        private void CargarGrid()
        {
            var lista = CN_tbl_proveedor.Listar(); 

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                lista = lista.Where(p => p.prov_nombre.ToLower().Contains(txtBuscar.Text.Trim().ToLower())).ToList();
            }

            // Filtro por estado
            if (!string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
            {
                char estado = ddlFiltroEstado.SelectedValue[0];
                lista = lista.Where(p => p.prov_estado == estado).ToList();
            }

            gvProveedores.DataSource = lista;
            gvProveedores.DataBind();
        }
        // ── Guardar / Actualizar ─────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfProvId.Value);
            string nombre = txtNombre.Text.Trim();

            // Validar nombre duplicado
            if (CN_tbl_proveedor.ExisteNombre(nombre, id))
            {
                MostrarMensaje("Ya existe un proveedor con ese nombre.", false);
                return;
            }

            try
            {
                if (id == 0)
                {
                    // Crear
                    CN_tbl_proveedor.Guardar(new tbl_proveedor { prov_nombre = nombre });
                    MostrarMensaje("✔ Proveedor creado correctamente.", true);
                }
                else
                {
                    // Editar
                    CN_tbl_proveedor.Modificar(new tbl_proveedor { prov_id = id, prov_nombre = nombre });
                    MostrarMensaje("✔ Proveedor actualizado.", true);
                }

                LimpiarFormulario();
                CargarGrid();
            }
            catch (Exception ex)
            {
                MostrarMensaje("❌ " + ex.Message, false);
            }
        }

        // ── Cancelar edición ─────────────────────────────────────────
        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // ── Comandos de fila (Editar / ElimLog / ElimFis) ────────────
        protected void gvProveedores_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());

            switch (e.CommandName)
            {
                case "Editar":
                    var prov = CN_tbl_proveedor.BuscarPorId(id);
                    if (prov != null)
                    {
                        hfProvId.Value = prov.prov_id.ToString();
                        txtNombre.Text = prov.prov_nombre;
                        litTituloForm.Text = "Editar Proveedor";
                        hfModalAbierto.Value = "1";
                    }
                    break;

                // Desactivar → estado 'I'
                case "ElimLog":
                    try { CN_tbl_proveedor.EliminarLogico(id); MostrarMensaje("Proveedor desactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                // Activar → estado 'A'
                case "Activar":
                    try { CN_tbl_proveedor.Activar(id); MostrarMensaje("Proveedor reactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                case "ElimFis":
                    try { CN_tbl_proveedor.EliminarFisico(id); MostrarMensaje("Proveedor eliminado permanentemente.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            hfProvId.Value    = "0";
            txtNombre.Text    = "";
            litTituloForm.Text = "➕ Nuevo Proveedor";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            string css = exito ? "alert alert-success" : "alert alert-danger";
            litMensaje.Text = $"<div class='{css}'>{texto}</div>";
        }
        protected void Buscar_Changed(object sender, EventArgs e) => CargarGrid();
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            CargarGrid();
        }
    }
}
