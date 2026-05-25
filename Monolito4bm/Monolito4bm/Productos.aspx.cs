using Capa_Datos;
using Capa_Negocios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Monolito4bm
{
    public partial class Productos : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Literal litMensaje;
        protected global::System.Web.UI.WebControls.TextBox txtBuscar;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroProveedor;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;
        protected global::System.Web.UI.WebControls.TextBox txtPrecioMin;
        protected global::System.Web.UI.WebControls.TextBox txtPrecioMax;
        protected global::System.Web.UI.WebControls.TextBox txtStockMin;
        protected global::System.Web.UI.WebControls.TextBox txtStockMax;
        protected global::System.Web.UI.WebControls.LinkButton btnLimpiarFiltros;
        protected global::System.Web.UI.WebControls.LinkButton btnGuardarProd;
        protected global::System.Web.UI.WebControls.LinkButton btnNuevo;
        protected global::System.Web.UI.WebControls.Literal litTotal;
        protected global::System.Web.UI.WebControls.HiddenField hfPagina;
        protected global::System.Web.UI.WebControls.HiddenField hfTotalPags;
        protected global::System.Web.UI.WebControls.GridView gvProductos;
        protected global::System.Web.UI.WebControls.Literal litPagerInfo;
        protected global::System.Web.UI.WebControls.Button btnPrev;
        protected global::System.Web.UI.WebControls.Repeater rptPager;
        protected global::System.Web.UI.WebControls.Button btnNext;
        protected global::System.Web.UI.WebControls.Literal litTituloModal;
        protected global::System.Web.UI.WebControls.HiddenField hfProdId;
        protected global::System.Web.UI.WebControls.TextBox txtNombre;
        protected global::System.Web.UI.WebControls.TextBox txtCantidad;
        protected global::System.Web.UI.WebControls.TextBox txtPrecio;
        protected global::System.Web.UI.WebControls.DropDownList ddlProveedor;
        protected global::System.Web.UI.WebControls.HiddenField hfModalAbierto;
        protected global::System.Web.UI.WebControls.HiddenField hfFiltrosAbiertos;

        private const int POR_PAGINA = 5;

        // ── Page_Load ────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombos();
                CargarGrid();
            }
        }

        // ── Combos ───────────────────────────────────────────────────
        private void CargarCombos()
        {
            var provs = CN_tbl_proveedor.Listar().Where(p => p.prov_estado == 'A').ToList();

            ddlFiltroProveedor.DataSource = provs;
            ddlFiltroProveedor.DataTextField = "prov_nombre";
            ddlFiltroProveedor.DataValueField = "prov_id";
            ddlFiltroProveedor.DataBind();
            ddlFiltroProveedor.Items.Insert(0, new ListItem("Todos los proveedores", ""));

            ddlProveedor.DataSource = provs;
            ddlProveedor.DataTextField = "prov_nombre";
            ddlProveedor.DataValueField = "prov_id";
            ddlProveedor.DataBind();
            ddlProveedor.Items.Insert(0, new ListItem("Seleccionar proveedor", ""));
        }

        // ── Grid paginado ─────────────────────────────────────────────
        private void CargarGrid()
        {
            int pagina = int.Parse(hfPagina.Value);
            int total = 0;

            string nombre = txtBuscar.Text.Trim();
            int? provId = string.IsNullOrEmpty(ddlFiltroProveedor.SelectedValue)
                            ? (int?)null : int.Parse(ddlFiltroProveedor.SelectedValue);
            char? estado = string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue)
                            ? (char?)null : ddlFiltroEstado.SelectedValue[0];
            decimal? pMin = string.IsNullOrEmpty(txtPrecioMin.Text) ? (decimal?)null : decimal.Parse(txtPrecioMin.Text);
            decimal? pMax = string.IsNullOrEmpty(txtPrecioMax.Text) ? (decimal?)null : decimal.Parse(txtPrecioMax.Text);
            int? sMin = string.IsNullOrEmpty(txtStockMin.Text) ? (int?)null : int.Parse(txtStockMin.Text);
            int? sMax = string.IsNullOrEmpty(txtStockMax.Text) ? (int?)null : int.Parse(txtStockMax.Text); 

            var lista = CN_tbl_producto.BuscarPaginado(
                pagina, POR_PAGINA, out total, nombre, provId, estado, pMin, pMax, sMin, sMax);

            gvProductos.DataSource = lista;
            gvProductos.DataBind();

            litTotal.Text = $"Total: {total} producto(s)";

            int totalPags = Math.Max(1, (int)Math.Ceiling((double)total / POR_PAGINA));
            hfTotalPags.Value = totalPags.ToString();
            litPagerInfo.Text = $"Pagina {pagina} de {totalPags}";
            btnPrev.Enabled = pagina > 1;
            btnNext.Enabled = pagina < totalPags;

            rptPager.DataSource = Enumerable.Range(1, totalPags).ToList();
            rptPager.DataBind();
        }

        // ── Búsqueda / filtros ────────────────────────────────────────
        protected void Buscar_Changed(object sender, EventArgs e)
        {
            hfPagina.Value = "1";
            CargarGrid();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = ""; txtPrecioMin.Text = ""; txtPrecioMax.Text = "";
            txtStockMin.Text = ""; txtStockMax.Text = "";
            ddlFiltroProveedor.SelectedIndex = 0;
            ddlFiltroEstado.SelectedIndex = 0;
            hfPagina.Value = "1";
            CargarGrid();
        }

        // ── Paginación ────────────────────────────────────────────────
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
            { hfPagina.Value = e.CommandArgument.ToString(); CargarGrid(); }
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            hfModalAbierto.Value = "1";
            ScriptManager.RegisterStartupScript(this, GetType(), "openNewProducto", "abrirModal();", true);
        }

        // ── Comandos de fila ──────────────────────────────────────────
        protected void gvProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());

            switch (e.CommandName)
            {
                case "Editar":
                    var prod = CN_tbl_producto.BuscarPorId(id);
                    if (prod != null)
                    {
                        hfProdId.Value = prod.pro_id.ToString();
                        txtNombre.Text = prod.pro_nombre;
                        txtCantidad.Text = prod.pro_cantidad.ToString();
                        txtPrecio.Text = prod.pro_precio.HasValue
                                              ? prod.pro_precio.Value.ToString("0.00") : "0.00";
                        
                        if (prod.prov_id.HasValue)
                        {
                            var item = ddlProveedor.Items.FindByValue(prod.prov_id.Value.ToString());
                            if (item != null)
                            {
                                ddlProveedor.SelectedValue = prod.prov_id.Value.ToString();
                            }
                            else
                            {
                                ddlProveedor.SelectedValue = "";
                            }
                        }
                        else
                        {
                            ddlProveedor.SelectedValue = "";
                        }

                        litTituloModal.Text = "Editar Producto";
                        hfModalAbierto.Value = "1";
                    }
                    break;

                // Desactivar
                case "ElimLog":
                    try { CN_tbl_producto.EliminarLogico(id); MostrarMensaje("Producto desactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                // Activar
                case "Activar":
                    try { CN_tbl_producto.Activar(id); MostrarMensaje("Producto reactivado.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;

                case "ElimFis":
                    try { CN_tbl_producto.EliminarFisico(id); MostrarMensaje("Producto eliminado permanentemente.", true); CargarGrid(); }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;
            }
        }

        // ── Guardar / Actualizar ──────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int id = 0;
            int.TryParse(hfProdId.Value, out id);
            string nombre = txtNombre.Text.Trim();

            if (CN_tbl_producto.ExisteNombre(nombre, id))
            {
                MostrarMensaje("Ya existe un producto activo con ese nombre.", false);
                hfModalAbierto.Value = "1";
                ScriptManager.RegisterStartupScript(this, GetType(), "openModalExNombre", "abrirModal();", true);
                return;
            }

            try
            {
                // TRUCO: Reemplazamos la coma por punto y forzamos la cultura invariante
                // Así nos aseguramos de que C# siempre lo entienda como decimal correcto.
                string precioLimpio = txtPrecio.Text.Replace(",", ".");
                decimal precioFinal = 0;
                decimal.TryParse(precioLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out precioFinal);

                int cantidad = 0;
                int.TryParse(txtCantidad.Text, out cantidad);

                var p = new tbl_producto
                {
                    pro_id = id,
                    pro_nombre = nombre,
                    pro_cantidad = cantidad,
                    pro_precio = precioFinal, // <-- Asignamos el valor limpio aquí
                    prov_id = int.Parse(ddlProveedor.SelectedValue)
                };

                if (id == 0) CN_tbl_producto.Guardar(p);
                else CN_tbl_producto.Modificar(p);

                MostrarMensaje(id == 0 ? "Producto creado correctamente." : "Producto actualizado.", true);
                hfModalAbierto.Value = "0";
                LimpiarFormulario();
                CargarGrid();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error: " + ex.Message, false);
                hfModalAbierto.Value = "1";
                ScriptManager.RegisterStartupScript(this, GetType(), "openModalError", "abrirModal();", true);
            }
        }
        // ── Carrusel HTML ─────────────────────────────────────────────
        public string GenerarCarrusel(object proId, object fotosObj)
        {
            var fotos = (fotosObj as System.Data.Linq.EntitySet<Capa_Datos.tbl_pro_fotos>)
                        ?.Where(f => f.foto_estado == 'A')   // solo fotos activas
                        .OrderByDescending(f => f.fecha_subida)
                        .ToList();

            if (fotos == null || !fotos.Any())
                return "<div class='no-foto'>" +
                       "<i class='fa-solid fa-camera' style='font-size:1.3rem;'></i>" +
                       "</div>";

            string html = "<div class='carousel-cell'>";
            for (int i = 0; i < fotos.Count; i++)
            {
                string act = i == 0 ? " active" : "";
                html += $"<div class='slide{act}'>" +
                        $"<img src='{ResolveUrl("~/" + fotos[i].foto_ruta)}'" +
                        $" alt='Foto {i + 1}'" +
                        $" onerror=\"this.src='https://placehold.co/110x80/ede6f8/7a4aaa?text=?'\"/>" +
                        "</div>";
            }
            if (fotos.Count > 1)
            {
                html += "<button type='button' class='prev'><i class='fa-solid fa-chevron-left'></i></button>";
                html += "<button type='button' class='next'><i class='fa-solid fa-chevron-right'></i></button>";
                html += "<div class='dots'>";
                for (int j = 0; j < fotos.Count; j++)
                    html += $"<div class='dot{(j == 0 ? " on" : "")}'></div>";
                html += "</div>";
            }
            html += "</div>";
            return html;
        }

        // ── Helpers ───────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            hfProdId.Value = "0"; txtNombre.Text = "";
            txtCantidad.Text = ""; txtPrecio.Text = "";
            ddlProveedor.SelectedIndex = 0;
            litTituloModal.Text = "Nuevo Producto";
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