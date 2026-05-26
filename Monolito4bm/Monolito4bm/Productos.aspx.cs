using Capa_Datos;
using Capa_Negocios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        protected global::System.Web.UI.WebControls.HiddenField hfDistribucionProductosJson;
        protected global::System.Web.UI.WebControls.HiddenField hfStockProductosJson;
        protected global::System.Web.UI.WebControls.HiddenField hfResumenProductosJson;

        private const int POR_PAGINA = 5;

        private sealed class ChartMetric
        {
            public string Label { get; set; }
            public decimal Value { get; set; }
        }

        private sealed class ProductSummary
        {
            public int TotalProducts { get; set; }
            public int ActiveProducts { get; set; }
            public int InactiveProducts { get; set; }
            public int ProductsWithPhotos { get; set; }
            public int ActivePercentage { get; set; }
        }

        private int ProductoEditandoId
        {
            get { return ViewState["ProductoEditandoId"] != null ? (int)ViewState["ProductoEditandoId"] : 0; }
            set { ViewState["ProductoEditandoId"] = value; }
        }

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
            ddlFiltroProveedor.Items.Insert(1, new ListItem("Sin proveedor", "-1"));

            ddlProveedor.DataSource = provs;
            ddlProveedor.DataTextField = "prov_nombre";
            ddlProveedor.DataValueField = "prov_id";
            ddlProveedor.DataBind();
            ddlProveedor.Items.Insert(0, new ListItem("Sin proveedor", ""));
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

            CargarEstadisticas();
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
                        ProductoEditandoId = prod.pro_id;
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
                        ScriptManager.RegisterStartupScript(this, GetType(), "openEditProducto", "abrirModal();", true);
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
                    try
                    {
                        var rutas = CN_tbl_producto.EliminarFisico(id);
                        foreach (var ruta in rutas)
                        {
                            string limpia = (ruta ?? string.Empty).TrimStart('~', '/').Replace("\\", "/");
                            if (string.IsNullOrWhiteSpace(limpia))
                            {
                                continue;
                            }

                            string rutaFisica = Server.MapPath("~/" + limpia);
                            if (System.IO.File.Exists(rutaFisica))
                            {
                                System.IO.File.Delete(rutaFisica);
                            }
                        }

                        MostrarMensaje("Producto eliminado permanentemente.", true);
                        CargarGrid();
                    }
                    catch (Exception ex) { MostrarMensaje(ex.Message, false); }
                    break;
            }
        }

        // ── Guardar / Actualizar ──────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(hfProdId.Value, out id))
            {
                id = ProductoEditandoId;
            }
            else if (id == 0 && ProductoEditandoId > 0)
            {
                id = ProductoEditandoId;
            }

            hfProdId.Value = id.ToString();
            string nombre = NormalizarNombreProducto(txtNombre.Text);

            if (CN_tbl_producto.ExisteNombre(nombre, id))
            {
                MostrarMensaje("Ya existe un producto activo con ese nombre.", false);
                hfModalAbierto.Value = "1";
                ScriptManager.RegisterStartupScript(this, GetType(), "openModalExNombre", "abrirModal();", true);
                return;
            }

            try
            {
                string precioLimpio = txtPrecio.Text.Replace(",", ".");
                decimal precioFinal = 0;
                decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out precioFinal);

                int cantidad = ParseCantidadProducto(txtCantidad.Text);

                var p = new tbl_producto
                {
                    pro_id = id,
                    pro_nombre = nombre,
                    pro_cantidad = cantidad,
                    pro_precio = precioFinal, // <-- Asignamos el valor limpio aquí
                    prov_id = string.IsNullOrWhiteSpace(ddlProveedor.SelectedValue) ? (int?)null : int.Parse(ddlProveedor.SelectedValue)
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
                string ruta = (fotos[i].foto_ruta ?? string.Empty).TrimStart('~', '/').Replace("\\", "/");
                html += $"<div class='slide{act}'>" +
                        $"<img src='{ResolveUrl("~/" + ruta)}'" +
                        $" alt='Foto {i + 1}'" +
                        $" onerror=\"this.onerror=null;this.src='ImagenProductoFallback.ashx?id={fotos[i].foto_id}';\"" +
                        "/>" +
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

        private void CargarEstadisticas()
        {
            using (var dc = new MonolitoDataContext())
            {
                var distribucionPorProveedor = (from producto in dc.GetTable<tbl_producto>()
                                                where producto.pro_estado == 'A'
                                                group producto by (producto.tbl_proveedor != null
                                                    ? producto.tbl_proveedor.prov_nombre
                                                    : "Sin proveedor") into grupo
                                                orderby grupo.Count() descending, grupo.Key
                                                select new ChartMetric
                                                {
                                                    Label = grupo.Key,
                                                    Value = grupo.Count()
                                                }).ToList();

                var stockPorProducto = (from producto in dc.GetTable<tbl_producto>()
                                        where producto.pro_estado == 'A'
                                        orderby (producto.pro_cantidad ?? 0) descending, producto.pro_nombre
                                        select new ChartMetric
                                        {
                                            Label = producto.pro_nombre,
                                            Value = producto.pro_cantidad ?? 0
                                        }).Take(8).ToList();

                int totalProducts = dc.GetTable<tbl_producto>().Count();
                int activeProducts = dc.GetTable<tbl_producto>().Count(p => p.pro_estado == 'A');
                int inactiveProducts = totalProducts - activeProducts;
                int productsWithPhotos = dc.GetTable<tbl_producto>()
                    .Count(p => p.tbl_pro_fotos.Any(f => f.foto_estado == 'A'));

                var resumen = new ProductSummary
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    InactiveProducts = inactiveProducts,
                    ProductsWithPhotos = productsWithPhotos,
                    ActivePercentage = totalProducts == 0
                        ? 0
                        : (int)Math.Round((activeProducts * 100m) / totalProducts, MidpointRounding.AwayFromZero)
                };

                hfDistribucionProductosJson.Value = JsonConvert.SerializeObject(distribucionPorProveedor);
                hfStockProductosJson.Value = JsonConvert.SerializeObject(stockPorProducto);
                hfResumenProductosJson.Value = JsonConvert.SerializeObject(resumen);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            ProductoEditandoId = 0;
            hfProdId.Value = "0"; txtNombre.Text = "";
            txtCantidad.Text = ""; txtPrecio.Text = "";
            ddlProveedor.SelectedIndex = 0;
            litTituloModal.Text = "Nuevo Producto";
        }

        private static string NormalizarNombreProducto(string valor)
        {
            return (valor ?? string.Empty).Trim().TrimEnd(',', ';');
        }

        private static int ParseCantidadProducto(string valor)
        {
            string texto = (valor ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto))
            {
                return 0;
            }

            int entero;
            if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.CurrentCulture, out entero) ||
                int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out entero))
            {
                return entero;
            }

            string normalizado = texto.Replace(" ", string.Empty).Replace(",", ".");
            decimal decimalCantidad;
            if (decimal.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalCantidad))
            {
                return (int)Math.Round(decimalCantidad, MidpointRounding.AwayFromZero);
            }

            return 0;
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
