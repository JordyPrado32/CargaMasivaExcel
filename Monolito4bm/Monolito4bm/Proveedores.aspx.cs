using Capa_Datos;
using Capa_Negocios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
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
        protected global::System.Web.UI.WebControls.GridView gvProveedores;
        protected global::System.Web.UI.WebControls.TextBox txtBuscar;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;
        protected global::System.Web.UI.WebControls.HiddenField hfModalAbierto;
        protected global::System.Web.UI.WebControls.HiddenField hfFiltrosAbiertos;
        protected global::System.Web.UI.WebControls.LinkButton btnLimpiarFiltros;
        protected global::System.Web.UI.WebControls.Literal litTotal;
        protected global::System.Web.UI.WebControls.HiddenField hfPagina;
        protected global::System.Web.UI.WebControls.HiddenField hfTotalPags;
        protected global::System.Web.UI.WebControls.Literal litPagerInfo;
        protected global::System.Web.UI.WebControls.Button btnPrev;
        protected global::System.Web.UI.WebControls.Repeater rptPager;
        protected global::System.Web.UI.WebControls.Button btnNext;
        protected global::System.Web.UI.WebControls.HiddenField hfDistribucionProductosJson;
        protected global::System.Web.UI.WebControls.HiddenField hfStockProveedoresJson;
        protected global::System.Web.UI.WebControls.HiddenField hfResumenProveedoresJson;

        private const int POR_PAGINA = 5;

        private sealed class ChartMetric
        {
            public string Label { get; set; }
            public int Value { get; set; }
        }

        private sealed class ProviderSummary
        {
            public int TotalProviders { get; set; }
            public int ActiveProviders { get; set; }
            public int InactiveProviders { get; set; }
            public int ProvidersWithProducts { get; set; }
            public int ActivePercentage { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarGrid();
            }
        }

        private void CargarGrid()
        {
            int pagina = ObtenerPaginaActual();
            var lista = CN_tbl_proveedor.Listar();

            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                string termino = txtBuscar.Text.Trim();
                lista = lista
                    .Where(p => !string.IsNullOrWhiteSpace(p.prov_nombre) &&
                                p.prov_nombre.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
            {
                char estado = ddlFiltroEstado.SelectedValue[0];
                lista = lista.Where(p => p.prov_estado == estado).ToList();
            }

            int total = lista.Count;
            int totalPags = Math.Max(1, (int)Math.Ceiling((double)total / POR_PAGINA));

            if (pagina > totalPags)
            {
                pagina = totalPags;
            }

            hfPagina.Value = pagina.ToString();
            hfTotalPags.Value = totalPags.ToString();

            litTotal.Text = $"Total: {total} proveedor(es)";
            litPagerInfo.Text = $"Página {pagina} de {totalPags}";

            btnPrev.Enabled = pagina > 1;
            btnNext.Enabled = pagina < totalPags;

            rptPager.DataSource = Enumerable.Range(1, totalPags).ToList();
            rptPager.DataBind();

            var listaPaginada = lista
                .Skip((pagina - 1) * POR_PAGINA)
                .Take(POR_PAGINA)
                .ToList();

            gvProveedores.DataSource = listaPaginada;
            gvProveedores.DataBind();

            CargarEstadisticas();
        }

        private void CargarEstadisticas()
        {
            using (var dc = new MonolitoDataContext())
            {
                var distribucionProductos = (from producto in dc.GetTable<tbl_producto>()
                                             where producto.pro_estado == 'A' && producto.tbl_proveedor != null
                                             group producto by new
                                             {
                                                 producto.prov_id,
                                                 producto.tbl_proveedor.prov_nombre
                                             } into grupo
                                             orderby grupo.Count() descending, grupo.Key.prov_nombre
                                             select new ChartMetric
                                             {
                                                 Label = grupo.Key.prov_nombre,
                                                 Value = grupo.Count()
                                             }).ToList();

                var stockPorProveedor = (from producto in dc.GetTable<tbl_producto>()
                                         where producto.pro_estado == 'A' && producto.tbl_proveedor != null
                                         group producto by new
                                         {
                                             producto.prov_id,
                                             producto.tbl_proveedor.prov_nombre
                                         } into grupo
                                         let stockTotal = grupo.Sum(item => item.pro_cantidad ?? 0)
                                         orderby stockTotal descending, grupo.Key.prov_nombre
                                         select new ChartMetric
                                         {
                                             Label = grupo.Key.prov_nombre,
                                             Value = stockTotal
                                         }).ToList();

                int totalProviders = dc.GetTable<tbl_proveedor>().Count();
                int activeProviders = dc.GetTable<tbl_proveedor>().Count(p => p.prov_estado == 'A');
                int inactiveProviders = totalProviders - activeProviders;
                int activePercentage = totalProviders == 0
                    ? 0
                    : (int)Math.Round((activeProviders * 100m) / totalProviders, MidpointRounding.AwayFromZero);

                var resumen = new ProviderSummary
                {
                    TotalProviders = totalProviders,
                    ActiveProviders = activeProviders,
                    InactiveProviders = inactiveProviders,
                    ProvidersWithProducts = distribucionProductos.Count,
                    ActivePercentage = activePercentage
                };

                hfDistribucionProductosJson.Value = JsonConvert.SerializeObject(distribucionProductos);
                hfStockProveedoresJson.Value = JsonConvert.SerializeObject(stockPorProveedor);
                hfResumenProveedoresJson.Value = JsonConvert.SerializeObject(resumen);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfProvId.Value);
            string nombre = (txtNombre.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarMensaje("El nombre del proveedor es obligatorio.", false);
                hfModalAbierto.Value = "1";
                return;
            }

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
                    CN_tbl_proveedor.Guardar(new tbl_proveedor { prov_nombre = nombre });
                    MostrarMensaje("Proveedor creado correctamente.", true);
                }
                else
                {
                    CN_tbl_proveedor.Modificar(new tbl_proveedor { prov_id = id, prov_nombre = nombre });
                    MostrarMensaje("Proveedor actualizado.", true);
                }

                LimpiarFormulario();
                hfModalAbierto.Value = "0";
                CargarGrid();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, false);
                hfModalAbierto.Value = "1";
            }
        }

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
                    try
                    {
                        CN_tbl_proveedor.EliminarLogico(id);
                        MostrarMensaje("Proveedor desactivado.", true);
                        CargarGrid();
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje(ex.Message, false);
                    }
                    break;

                case "Activar":
                    try
                    {
                        CN_tbl_proveedor.Activar(id);
                        MostrarMensaje("Proveedor reactivado.", true);
                        CargarGrid();
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje(ex.Message, false);
                    }
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
                        MostrarMensaje(ex.Message, false);
                    }
                    break;
            }
        }

        protected void Buscar_Changed(object sender, EventArgs e)
        {
            hfPagina.Value = "1";
            CargarGrid();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            ddlFiltroEstado.SelectedIndex = 0;
            hfPagina.Value = "1";
            CargarGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            int pagina = ObtenerPaginaActual();
            if (pagina > 1)
            {
                hfPagina.Value = (pagina - 1).ToString();
                CargarGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int pagina = ObtenerPaginaActual();
            int maximo = ObtenerEnteroSeguro(hfTotalPags.Value, 1);

            if (pagina < maximo)
            {
                hfPagina.Value = (pagina + 1).ToString();
                CargarGrid();
            }
        }

        protected void rptPager_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Paginar")
            {
                hfPagina.Value = e.CommandArgument.ToString();
                CargarGrid();
            }
        }

        private int ObtenerPaginaActual()
        {
            return Math.Max(1, ObtenerEnteroSeguro(hfPagina.Value, 1));
        }

        private static int ObtenerEnteroSeguro(string valor, int valorPorDefecto)
        {
            int numero;
            return int.TryParse(valor, out numero) ? numero : valorPorDefecto;
        }

        private void LimpiarFormulario()
        {
            hfProvId.Value = "0";
            txtNombre.Text = string.Empty;
            litTituloForm.Text = "➕ Nuevo Proveedor";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            string icon = exito ? "success" : "error";
            string title = exito ? "¡Éxito!" : "¡Atención!";
            string safeTitle = HttpUtility.JavaScriptStringEncode(title);
            string safeText = HttpUtility.JavaScriptStringEncode(texto ?? string.Empty);
            string script = $"Swal.fire({{ title: '{safeTitle}', text: '{safeText}', icon: '{icon}', confirmButtonColor: '#2563eb' }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "swal_msg", script, true);

            string css = exito ? "alert alert-success" : "alert alert-danger";
            litMensaje.Text = $"<div class='{css}'>{HttpUtility.HtmlEncode(texto)}</div>";
        }
    }
}
