using Capa_Datos;
using Capa_Negocios;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Monolito4bm
{
    public partial class Proveedores : System.Web.UI.Page
    {
        // Controles de ASPX
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

        // Paginación
        protected global::System.Web.UI.WebControls.Literal litTotal;
        protected global::System.Web.UI.WebControls.HiddenField hfPagina;
        protected global::System.Web.UI.WebControls.HiddenField hfTotalPags;
        protected global::System.Web.UI.WebControls.Literal litPagerInfo;
        protected global::System.Web.UI.WebControls.Button btnPrev;
        protected global::System.Web.UI.WebControls.Repeater rptPager;
        protected global::System.Web.UI.WebControls.Button btnNext;

        private const int POR_PAGINA = 5;

        // ── Page_Load ────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarGrid();
        }

        // ── Llenar GridView con Paginación en Memoria ────────────────
        private void CargarGrid()
        {
            int pagina = int.Parse(hfPagina.Value);

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

            // Lógica de paginación y totales
            int total = lista.Count;
            litTotal.Text = $"Total: {total} proveedor(es)";

            int totalPags = Math.Max(1, (int)Math.Ceiling((double)total / POR_PAGINA));
            hfTotalPags.Value = totalPags.ToString();
            litPagerInfo.Text = $"Página {pagina} de {totalPags}";

            btnPrev.Enabled = pagina > 1;
            btnNext.Enabled = pagina < totalPags;

            rptPager.DataSource = Enumerable.Range(1, totalPags).ToList();
            rptPager.DataBind();

            // Paginamos en memoria antes de mandar al Grid
            var listaPaginada = lista.Skip((pagina - 1) * POR_PAGINA).Take(POR_PAGINA).ToList();

            gvProveedores.DataSource = listaPaginada;
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
                hfModalAbierto.Value = "1";
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
                hfModalAbierto.Value = "0";
                CargarGrid();
            }
            catch (Exception ex)
            {
                MostrarMensaje("❌ " + ex.Message, false);
                hfModalAbierto.Value = "1";
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

                case "ElimLog":
                    try { CN_tbl_proveedor.EliminarLogico(id); MostrarMensaje("Proveedor desactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                case "Activar":
                    try { CN_tbl_proveedor.Activar(id); MostrarMensaje("Proveedor reactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                case "ElimFis":
                    try
                    {
                        CN_tbl_proveedor.EliminarFisico(id);
                        MostrarMensaje("Proveedor eliminado permanentemente.", true);
                        CargarGrid();
                    }
                    catch (Exception ex)
                    {
                        // Aquí llegará el mensaje "No se puede borrar..." que lanzamos desde la Capa de Negocios
                        MostrarMensaje(ex.Message, false);
                    }
                    break;
            }
        }

        // ── Búsqueda y Filtros ───────────────────────────────────────
        protected void Buscar_Changed(object sender, EventArgs e)
        {
            hfPagina.Value = "1"; // Volver a la página 1 al filtrar
            CargarGrid();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            hfPagina.Value = "1"; // Volver a la página 1 al limpiar
            CargarGrid();
        }

        // ── Paginación Eventos ───────────────────────────────────────
        protected void btnPrev_Click(object sender, EventArgs e)
        {
            int p = int.Parse(hfPagina.Value);
            if (p > 1) { hfPagina.Value = (p - 1).ToString(); CargarGrid(); }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int p = int.Parse(hfPagina.Value), m = int.Parse(hfTotalPags.Value);
            if (p < m) { hfPagina.Value = (p + 1).ToString(); CargarGrid(); }
        }

        protected void rptPager_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Paginar")
            {
                hfPagina.Value = e.CommandArgument.ToString();
                CargarGrid();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            hfProvId.Value = "0";
            txtNombre.Text = "";
            litTituloForm.Text = "➕ Nuevo Proveedor";
        }
        private void MostrarMensaje(string texto, bool exito)
        {
            string icon = exito ? "success" : "error";
            string title = exito ? "¡Éxito!" : "¡Atención!";
            string script = $"Swal.fire({{ title: '{title}', text: '{texto}', icon: '{icon}', confirmButtonColor: '#7a4aaa' }});";

            // 🔴 BORRA O COMENTA ESTA LÍNEA:
            // ClientScript.RegisterStartupScript(this.GetType(), "swal_msg_fotos", script, true);

            // 🟢 USA ESTA NUEVA LÍNEA (Compatible con UpdatePanel):
            ScriptManager.RegisterStartupScript(this, this.GetType(), "swal_msg", script, true);

            string css = exito ? "alert alert-success" : "alert alert-danger";
            litMensaje.Text = $"<div class='{css}'>{texto}</div>";
        }
    }
}