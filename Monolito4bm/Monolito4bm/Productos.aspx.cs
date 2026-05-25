using Capa_Datos;
using Capa_Negocios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Monolito4bm
{
    public partial class Productos : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Literal litMensaje;
        protected global::System.Web.UI.WebControls.FileUpload fuCargaProductos;
        protected global::System.Web.UI.WebControls.HiddenField hfModoCarga;
        protected global::System.Web.UI.WebControls.Button btnCargaIncremental;
        protected global::System.Web.UI.WebControls.Button btnCargaTotal;
        protected global::System.Web.UI.WebControls.TextBox txtBuscar;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroProveedor;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;
        protected global::System.Web.UI.WebControls.TextBox txtPrecioMin;
        protected global::System.Web.UI.WebControls.TextBox txtPrecioMax;
        protected global::System.Web.UI.WebControls.TextBox txtStockMin;
        protected global::System.Web.UI.WebControls.TextBox txtStockMax;
        protected global::System.Web.UI.WebControls.LinkButton btnLimpiarFiltros;
        protected global::System.Web.UI.WebControls.LinkButton btnGuardarProd;
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
        protected global::System.Web.UI.WebControls.Literal litFotosModalInfo;
        protected global::System.Web.UI.WebControls.FileUpload fuFotosProducto;
        protected global::System.Web.UI.WebControls.HiddenField hfModalAbierto;
        protected global::System.Web.UI.WebControls.HiddenField hfFiltrosAbiertos;

        private const int POR_PAGINA = 5;
        private const int MAX_FOTOS_POR_PRODUCTO = 4;
        private const int MAX_BYTES_FOTO = 2 * 1024 * 1024;
        private const string CARPETA_FOTOS_PRODUCTO = "Uploads/Productos";
        private static readonly string[] ExtensionesPermitidas = { ".csv", ".xlsx", ".xls" };
        private static readonly char[] SeparadoresFotos = { '|', ';', '\n', '\r' };
        private static readonly string[] ExtensionesFotoPermitidas = { ".jpg", ".jpeg", ".png" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombos();
                CargarGrid();
            }
        }

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

        protected void Buscar_Changed(object sender, EventArgs e)
        {
            hfPagina.Value = "1";
            CargarGrid();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            txtPrecioMin.Text = "";
            txtPrecioMax.Text = "";
            txtStockMin.Text = "";
            txtStockMax.Text = "";
            ddlFiltroProveedor.SelectedIndex = 0;
            ddlFiltroEstado.SelectedIndex = 0;
            hfPagina.Value = "1";
            CargarGrid();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            int p = int.Parse(hfPagina.Value);
            if (p > 1)
            {
                hfPagina.Value = (p - 1).ToString();
                CargarGrid();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int p = int.Parse(hfPagina.Value);
            int m = int.Parse(hfTotalPags.Value);
            if (p < m)
            {
                hfPagina.Value = (p + 1).ToString();
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

        protected void gvProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());

            switch (e.CommandName)
            {
                case "Editar":
                    var prod = ObtenerProductoParaEdicion(id);
                    if (prod != null)
                    {
                        hfProdId.Value = prod.pro_id.ToString();
                        txtNombre.Text = prod.pro_nombre;
                        txtCantidad.Text = prod.pro_cantidad.ToString();
                        txtPrecio.Text = prod.pro_precio.HasValue
                            ? prod.pro_precio.Value.ToString("0.00")
                            : "0.00";
                        ddlProveedor.SelectedValue = prod.prov_id.ToString();
                        litFotosModalInfo.Text = ConstruirResumenFotosModal(prod.pro_id);
                        litTituloModal.Text = "Editar Producto";
                        hfModalAbierto.Value = "1";
                    }
                    break;
                case "ElimLog":
                    try
                    {
                        CN_tbl_producto.EliminarLogico(id);
                        MostrarMensaje("Producto desactivado.", true);
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
                        CN_tbl_producto.Activar(id);
                        MostrarMensaje("Producto reactivado.", true);
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
                        string mensaje = EliminarProductoConRelaciones(id);
                        MostrarMensaje(mensaje, true);
                        CargarGrid();
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje(ex.Message, false);
                    }
                    break;
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfProdId.Value);
            string nombre = txtNombre.Text.Trim();

            try
            {
                string precioLimpio = txtPrecio.Text.Replace(",", ".");
                decimal precioFinal = decimal.Parse(precioLimpio, CultureInfo.InvariantCulture);
                string mensaje = GuardarProductoConRelaciones(
                    id,
                    nombre,
                    int.Parse(txtCantidad.Text),
                    precioFinal,
                    int.Parse(ddlProveedor.SelectedValue));

                MostrarMensaje(mensaje, true);
                hfModalAbierto.Value = "0";
                LimpiarFormulario();
                CargarGrid();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error: " + ex.Message, false);
                hfModalAbierto.Value = "1";
            }
        }

        private tbl_producto ObtenerProductoParaEdicion(int productoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.tbl_producto.FirstOrDefault(p => p.pro_id == productoId);
            }
        }

        private string GuardarProductoConRelaciones(int productoId, string nombre, int cantidad, decimal precio, int proveedorId)
        {
            var archivosGuardados = new List<string>();
            int fotosRegistradas = 0;

            using (var dc = new MonolitoDataContext())
            {
                dc.Connection.Open();
                using (var transaction = dc.Connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dc.Transaction = transaction;
                    try
                    {
                        string nombreNormalizado = (nombre ?? string.Empty).Trim();
                        if (dc.tbl_producto.Any(p => p.pro_id != productoId &&
                                                     p.pro_estado == 'A' &&
                                                     p.pro_nombre == nombreNormalizado))
                        {
                            throw new InvalidOperationException("Ya existe un producto activo con ese nombre.");
                        }

                        var proveedor = dc.tbl_proveedor
                            .FirstOrDefault(p => p.prov_id == proveedorId && p.prov_estado == 'A');
                        if (proveedor == null)
                        {
                            throw new InvalidOperationException("Selecciona un proveedor activo y vÃ¡lido.");
                        }

                        tbl_producto producto;
                        if (productoId == 0)
                        {
                            producto = new tbl_producto
                            {
                                pro_nombre = nombreNormalizado,
                                pro_cantidad = cantidad,
                                pro_precio = precio,
                                pro_estado = 'A',
                                prov_id = proveedor.prov_id
                            };

                            dc.tbl_producto.InsertOnSubmit(producto);
                            dc.SubmitChanges();
                        }
                        else
                        {
                            producto = dc.tbl_producto.FirstOrDefault(p => p.pro_id == productoId)
                                ?? throw new InvalidOperationException("Producto no encontrado.");

                            producto.pro_nombre = nombreNormalizado;
                            producto.pro_cantidad = cantidad;
                            producto.pro_precio = precio;
                            producto.prov_id = proveedor.prov_id;
                            dc.SubmitChanges();
                        }

                        var nuevasFotos = RegistrarFotosDesdeModal(dc, producto.pro_id, archivosGuardados);
                        if (nuevasFotos.Any())
                        {
                            dc.tbl_pro_fotos.InsertAllOnSubmit(nuevasFotos);
                            dc.SubmitChanges();
                            fotosRegistradas = nuevasFotos.Count;
                        }

                        transaction.Commit();

                        string accion = productoId == 0 ? "Producto creado correctamente." : "Producto actualizado.";
                        if (fotosRegistradas > 0)
                        {
                            accion += $" Fotos registradas: {fotosRegistradas}.";
                        }

                        return accion;
                    }
                    catch
                    {
                        transaction.Rollback();
                        EliminarArchivosFisicos(archivosGuardados);
                        throw;
                    }
                    finally
                    {
                        dc.Connection.Close();
                    }
                }
            }
        }

        private List<tbl_pro_fotos> RegistrarFotosDesdeModal(MonolitoDataContext dc, int productoId, List<string> archivosGuardados)
        {
            if (fuFotosProducto == null || !fuFotosProducto.HasFiles)
            {
                return new List<tbl_pro_fotos>();
            }

            var archivos = fuFotosProducto.PostedFiles
                .Cast<System.Web.HttpPostedFile>()
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            if (!archivos.Any())
            {
                return new List<tbl_pro_fotos>();
            }

            int fotosActuales = dc.tbl_pro_fotos.Count(f => f.pro_id == productoId);
            if (fotosActuales + archivos.Count > MAX_FOTOS_POR_PRODUCTO)
            {
                throw new InvalidOperationException($"Este producto solo admite {MAX_FOTOS_POR_PRODUCTO} fotos en total.");
            }

            string carpetaFisica = Server.MapPath("~/" + CARPETA_FOTOS_PRODUCTO);
            if (!Directory.Exists(carpetaFisica))
            {
                Directory.CreateDirectory(carpetaFisica);
            }

            var nuevasFotos = new List<tbl_pro_fotos>();
            foreach (var archivo in archivos)
            {
                if (archivo.ContentLength > MAX_BYTES_FOTO)
                {
                    throw new InvalidOperationException($"La foto \"{Path.GetFileName(archivo.FileName)}\" supera los 2 MB permitidos.");
                }

                string extension = Path.GetExtension(archivo.FileName ?? string.Empty).ToLowerInvariant();
                if (!ExtensionesFotoPermitidas.Contains(extension))
                {
                    throw new InvalidOperationException($"La foto \"{Path.GetFileName(archivo.FileName)}\" no tiene una extensiÃ³n permitida.");
                }

                string contentType = (archivo.ContentType ?? string.Empty).ToLowerInvariant();
                if (contentType != "image/jpeg" && contentType != "image/png")
                {
                    throw new InvalidOperationException($"La foto \"{Path.GetFileName(archivo.FileName)}\" debe ser JPG o PNG.");
                }

                string nombreArchivo = $"prod_{productoId}_{Guid.NewGuid():N}{(extension == ".jpeg" ? ".jpg" : extension)}";
                string rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);
                archivo.SaveAs(rutaFisica);
                archivosGuardados.Add(rutaFisica);

                nuevasFotos.Add(new tbl_pro_fotos
                {
                    pro_id = productoId,
                    foto_ruta = $"{CARPETA_FOTOS_PRODUCTO}/{nombreArchivo}",
                    foto_estado = 'A',
                    fecha_subida = DateTime.Now
                });
            }

            return nuevasFotos;
        }

        private string EliminarProductoConRelaciones(int productoId)
        {
            var rutasFoto = new List<string>();
            using (var dc = new MonolitoDataContext())
            {
                dc.Connection.Open();
                using (var transaction = dc.Connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dc.Transaction = transaction;
                    try
                    {
                        var producto = dc.tbl_producto.FirstOrDefault(p => p.pro_id == productoId)
                            ?? throw new InvalidOperationException("Producto no encontrado.");

                        var fotos = dc.tbl_pro_fotos.Where(f => f.pro_id == productoId).ToList();
                        rutasFoto.AddRange(fotos
                            .Select(f => f.foto_ruta)
                            .Where(r => !string.IsNullOrWhiteSpace(r))
                            .Distinct(StringComparer.OrdinalIgnoreCase));

                        if (fotos.Any())
                        {
                            dc.tbl_pro_fotos.DeleteAllOnSubmit(fotos);
                            dc.SubmitChanges();
                        }

                        dc.tbl_producto.DeleteOnSubmit(producto);
                        dc.SubmitChanges();
                        transaction.Commit();
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException("No se pudo eliminar el producto porque todavÃ­a tiene relaciones pendientes.");
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dc.Connection.Close();
                    }
                }
            }

            EliminarArchivosFisicos(rutasFoto.Select(ResolverRutaFisicaDesdeBase));
            return "Producto eliminado permanentemente con sus fotos asociadas.";
        }

        private string ConstruirResumenFotosModal(int productoId)
        {
            using (var dc = new MonolitoDataContext())
            {
                int total = dc.tbl_pro_fotos.Count(f => f.pro_id == productoId);
                int disponibles = Math.Max(0, MAX_FOTOS_POR_PRODUCTO - total);
                return $"Actualmente hay {total} foto(s) registradas. Puedes agregar hasta {disponibles} mÃ¡s en este guardado o seguir administrÃ¡ndolas desde la galerÃ­a del producto.";
            }
        }

        private string ResolverRutaFisicaDesdeBase(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return string.Empty;
            }

            return Server.MapPath("~/" + rutaRelativa.TrimStart('~', '/').Replace("\\", "/"));
        }

        private void EliminarArchivosFisicos(IEnumerable<string> rutasFisicas)
        {
            foreach (string ruta in (rutasFisicas ?? Enumerable.Empty<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                }
            }
        }

        protected void btnCargaMasiva_Click(object sender, EventArgs e)
        {
            hfModalAbierto.Value = "0";

            try
            {
                string extension = ObtenerExtensionValida();
                string modo = (hfModoCarga.Value ?? string.Empty).Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(modo))
                {
                    throw new InvalidOperationException("No se seleccionó un modo de procesamiento.");
                }

                var filas = LeerArchivo(fuCargaProductos.PostedFile, extension);
                if (!filas.Any())
                {
                    throw new InvalidOperationException("El archivo no contiene filas válidas para procesar.");
                }

                string resumen = modo == "overwrite"
                    ? EjecutarSobrescrituraCompleta(filas)
                    : EjecutarCargaIncremental(filas);

                CargarCombos();
                hfPagina.Value = "1";
                CargarGrid();
                MostrarMensaje(resumen, true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Carga masiva rechazada: " + ex.Message, false);
            }
            finally
            {
                hfModoCarga.Value = string.Empty;
            }
        }

        private string ObtenerExtensionValida()
        {
            if (fuCargaProductos == null || !fuCargaProductos.HasFile || fuCargaProductos.PostedFile == null)
            {
                throw new InvalidOperationException("Selecciona un archivo antes de iniciar la carga.");
            }

            string extension = Path.GetExtension(fuCargaProductos.FileName ?? string.Empty).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                throw new InvalidOperationException("Solo se permiten archivos .csv, .xlsx o .xls.");
            }

            return extension;
        }

        private List<ProductoImportRow> LeerArchivo(System.Web.HttpPostedFile archivo, string extension)
        {
            DataTable datos;
            switch (extension)
            {
                case ".csv":
                    datos = LeerCsv(archivo.InputStream);
                    break;
                case ".xlsx":
                    datos = LeerXlsx(archivo.InputStream);
                    break;
                case ".xls":
                    datos = LeerXls(archivo);
                    break;
                default:
                    throw new InvalidOperationException("Formato de archivo no soportado.");
            }

            return ConvertirFilas(datos);
        }

        private DataTable LeerCsv(Stream input)
        {
            var tabla = new DataTable();
            using (var reader = new StreamReader(input, Encoding.UTF8, true, 1024, true))
            {
                var lineas = new List<string>();
                while (!reader.EndOfStream)
                {
                    lineas.Add(reader.ReadLine());
                }

                if (!lineas.Any(l => !string.IsNullOrWhiteSpace(l)))
                {
                    return tabla;
                }

                string[] encabezados = ParsearLineaCsv(lineas.First(l => !string.IsNullOrWhiteSpace(l)));
                foreach (string encabezado in encabezados)
                {
                    tabla.Columns.Add((encabezado ?? string.Empty).Trim());
                }

                foreach (string linea in lineas.SkipWhile(l => string.IsNullOrWhiteSpace(l)).Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(linea))
                    {
                        continue;
                    }

                    string[] valores = ParsearLineaCsv(linea);
                    var fila = tabla.NewRow();
                    for (int i = 0; i < tabla.Columns.Count; i++)
                    {
                        fila[i] = i < valores.Length ? valores[i] : string.Empty;
                    }
                    tabla.Rows.Add(fila);
                }
            }

            return tabla;
        }

        private string[] ParsearLineaCsv(string linea)
        {
            var valores = new List<string>();
            var actual = new StringBuilder();
            bool enComillas = false;

            for (int i = 0; i < (linea ?? string.Empty).Length; i++)
            {
                char c = linea[i];
                if (c == '"')
                {
                    if (enComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                    {
                        actual.Append('"');
                        i++;
                    }
                    else
                    {
                        enComillas = !enComillas;
                    }
                }
                else if (c == ',' && !enComillas)
                {
                    valores.Add(actual.ToString().Trim());
                    actual.Clear();
                }
                else
                {
                    actual.Append(c);
                }
            }

            valores.Add(actual.ToString().Trim());
            return valores.ToArray();
        }

        private DataTable LeerXlsx(Stream input)
        {
            var tabla = new DataTable();
            using (var memory = new MemoryStream())
            {
                input.CopyTo(memory);
                memory.Position = 0;

                using (var archive = new ZipArchive(memory, ZipArchiveMode.Read, true))
                {
                    var sharedStrings = LeerSharedStrings(archive);
                    string hojaPath = ObtenerPrimeraHojaXlsx(archive);
                    if (string.IsNullOrEmpty(hojaPath))
                    {
                        throw new InvalidOperationException("No se encontró una hoja de cálculo válida dentro del archivo .xlsx.");
                    }

                    var hojaDoc = XDocument.Load(archive.GetEntry(hojaPath).Open());
                    XNamespace ns = hojaDoc.Root.Name.Namespace;
                    var filas = hojaDoc.Descendants(ns + "row").ToList();
                    if (!filas.Any())
                    {
                        return tabla;
                    }

                    var celdasEncabezado = filas.First().Elements(ns + "c").ToList();
                    int totalColumnas = celdasEncabezado.Any()
                        ? celdasEncabezado.Max(c => ObtenerIndiceColumna((string)c.Attribute("r"))) + 1
                        : 0;

                    for (int i = 0; i < totalColumnas; i++)
                    {
                        tabla.Columns.Add(LeerValorCelda(celdasEncabezado.FirstOrDefault(c => ObtenerIndiceColumna((string)c.Attribute("r")) == i), sharedStrings));
                    }

                    foreach (var filaXml in filas.Skip(1))
                    {
                        var fila = tabla.NewRow();
                        foreach (var celda in filaXml.Elements(ns + "c"))
                        {
                            int indice = ObtenerIndiceColumna((string)celda.Attribute("r"));
                            if (indice >= 0 && indice < tabla.Columns.Count)
                            {
                                fila[indice] = LeerValorCelda(celda, sharedStrings);
                            }
                        }
                        tabla.Rows.Add(fila);
                    }
                }
            }

            return tabla;
        }

        private List<string> LeerSharedStrings(ZipArchive archive)
        {
            var resultado = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return resultado;
            }

            var doc = XDocument.Load(entry.Open());
            XNamespace ns = doc.Root.Name.Namespace;
            resultado.AddRange(doc.Descendants(ns + "si").Select(ExtraerTextoCompartido));
            return resultado;
        }

        private string ExtraerTextoCompartido(XElement item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            XNamespace ns = item.Name.Namespace;
            return string.Concat(item.Descendants(ns + "t").Select(t => t.Value));
        }

        private string ObtenerPrimeraHojaXlsx(ZipArchive archive)
        {
            var workbookEntry = archive.GetEntry("xl/workbook.xml");
            var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (workbookEntry == null || relsEntry == null)
            {
                return null;
            }

            var workbookDoc = XDocument.Load(workbookEntry.Open());
            var relsDoc = XDocument.Load(relsEntry.Open());

            XNamespace wbNs = workbookDoc.Root.Name.Namespace;
            XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            var primeraHoja = workbookDoc.Descendants(wbNs + "sheet").FirstOrDefault();
            if (primeraHoja == null)
            {
                return null;
            }

            string relId = (string)primeraHoja.Attribute(relNs + "id");
            var relacion = relsDoc.Root.Elements()
                .FirstOrDefault(r => string.Equals((string)r.Attribute("Id"), relId, StringComparison.OrdinalIgnoreCase));
            if (relacion == null)
            {
                return null;
            }

            string target = ((string)relacion.Attribute("Target") ?? string.Empty).Replace("\\", "/");
            return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)
                ? target
                : "xl/" + target.TrimStart('/');
        }

        private int ObtenerIndiceColumna(string referenciaCelda)
        {
            if (string.IsNullOrWhiteSpace(referenciaCelda))
            {
                return -1;
            }

            string letras = new string(referenciaCelda.TakeWhile(char.IsLetter).ToArray()).ToUpperInvariant();
            int indice = 0;
            foreach (char letra in letras)
            {
                indice = (indice * 26) + (letra - 'A' + 1);
            }

            return indice - 1;
        }

        private string LeerValorCelda(XElement celda, IList<string> sharedStrings)
        {
            if (celda == null)
            {
                return string.Empty;
            }

            XNamespace ns = celda.Name.Namespace;
            string tipo = (string)celda.Attribute("t") ?? string.Empty;

            if (tipo == "inlineStr")
            {
                return string.Concat(celda.Descendants(ns + "t").Select(t => t.Value));
            }

            string valor = celda.Element(ns + "v")?.Value ?? string.Empty;
            if (tipo == "s" && int.TryParse(valor, out int indiceShared) && indiceShared >= 0 && indiceShared < sharedStrings.Count)
            {
                return sharedStrings[indiceShared];
            }

            return valor;
        }

        private DataTable LeerXls(System.Web.HttpPostedFile archivo)
        {
            string extension = Path.GetExtension(archivo.FileName ?? string.Empty);
            string temporal = Path.Combine(Path.GetTempPath(), $"productos_{Guid.NewGuid():N}{extension}");

            try
            {
                archivo.SaveAs(temporal);
                return LeerExcelConOleDb(temporal, extension);
            }
            catch (OleDbException)
            {
                throw new InvalidOperationException("No fue posible leer el archivo .xls. Verifica que el servidor tenga instalado el proveedor de Excel de Microsoft.");
            }
            finally
            {
                if (File.Exists(temporal))
                {
                    File.Delete(temporal);
                }
            }
        }

        private DataTable LeerExcelConOleDb(string rutaArchivo, string extension)
        {
            var conexiones = new List<string>();
            if (string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
            {
                conexiones.Add($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={rutaArchivo};Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\";");
                conexiones.Add($"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={rutaArchivo};Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\";");
            }
            else
            {
                conexiones.Add($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={rutaArchivo};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";");
            }

            Exception ultimoError = null;
            foreach (string connectionString in conexiones)
            {
                try
                {
                    using (var connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();
                        DataTable hojas = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        string hoja = hojas?.Rows.Cast<DataRow>()
                            .Select(r => r["TABLE_NAME"].ToString())
                            .FirstOrDefault(n => n.EndsWith("$") || n.EndsWith("$'"));

                        if (string.IsNullOrEmpty(hoja))
                        {
                            throw new InvalidOperationException("No se encontró ninguna hoja válida en el archivo Excel.");
                        }

                        using (var adapter = new OleDbDataAdapter($"SELECT * FROM [{hoja}]", connection))
                        {
                            var tabla = new DataTable();
                            adapter.Fill(tabla);
                            return tabla;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ultimoError = ex;
                }
            }

            throw ultimoError ?? new InvalidOperationException("No fue posible leer el archivo Excel.");
        }

        private List<ProductoImportRow> ConvertirFilas(DataTable tabla)
        {
            if (tabla == null || tabla.Columns.Count == 0)
            {
                return new List<ProductoImportRow>();
            }

            int colNombre = ObtenerIndiceColumna(tabla, "nombreproducto", "producto", "nombre", "pro_nombre");
            int colProveedor = ObtenerIndiceColumna(tabla, "proveedor", "nombreproveedor", "prov_nombre");
            int colCantidad = ObtenerIndiceColumna(tabla, "cantidad", "stock", "pro_cantidad");
            int colPrecio = ObtenerIndiceColumna(tabla, "precio", "valor", "pro_precio");
            int colImagen = ObtenerIndiceColumna(tabla, "imagenreferencia", "imageref", "fotoruta", "imagen", "foto", "foto_ruta");

            if (colNombre < 0 || colProveedor < 0 || colCantidad < 0 || colPrecio < 0)
            {
                throw new InvalidOperationException("El archivo debe incluir encabezados para producto, proveedor, cantidad y precio.");
            }

            var filas = new List<ProductoImportRow>();
            int numeroFila = 2;
            foreach (DataRow row in tabla.Rows)
            {
                string nombre = LeerTextoCelda(row, colNombre);
                string proveedor = LeerTextoCelda(row, colProveedor);
                string cantidadTexto = LeerTextoCelda(row, colCantidad);
                string precioTexto = LeerTextoCelda(row, colPrecio);
                string imagen = colImagen >= 0 ? LeerTextoCelda(row, colImagen) : string.Empty;

                if (string.IsNullOrWhiteSpace(nombre) &&
                    string.IsNullOrWhiteSpace(proveedor) &&
                    string.IsNullOrWhiteSpace(cantidadTexto) &&
                    string.IsNullOrWhiteSpace(precioTexto) &&
                    string.IsNullOrWhiteSpace(imagen))
                {
                    numeroFila++;
                    continue;
                }

                if (!int.TryParse(cantidadTexto, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cantidad) || cantidad < 0)
                {
                    throw new InvalidOperationException($"Fila {numeroFila}: la cantidad debe ser un entero no negativo.");
                }

                if (!TryParseDecimalFlexible(precioTexto, out decimal precio) || precio < 0)
                {
                    throw new InvalidOperationException($"Fila {numeroFila}: el precio no es válido.");
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new InvalidOperationException($"Fila {numeroFila}: el nombre del producto es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(proveedor))
                {
                    throw new InvalidOperationException($"Fila {numeroFila}: el proveedor es obligatorio.");
                }

                filas.Add(new ProductoImportRow
                {
                    RowNumber = numeroFila,
                    ProductName = nombre.Trim(),
                    ProviderName = proveedor.Trim(),
                    Quantity = cantidad,
                    Price = precio,
                    ImageReference = imagen?.Trim()
                });

                numeroFila++;
            }

            return filas;
        }

        private int ObtenerIndiceColumna(DataTable tabla, params string[] aliases)
        {
            var aliasSet = new HashSet<string>(aliases.Select(NormalizarNombreColumna));
            for (int i = 0; i < tabla.Columns.Count; i++)
            {
                if (aliasSet.Contains(NormalizarNombreColumna(tabla.Columns[i].ColumnName)))
                {
                    return i;
                }
            }

            return -1;
        }

        private string NormalizarNombreColumna(string value)
        {
            return Regex.Replace((value ?? string.Empty).Trim().ToLowerInvariant(), @"[^a-z0-9]", string.Empty);
        }

        private string LeerTextoCelda(DataRow row, int indice)
        {
            if (indice < 0 || indice >= row.Table.Columns.Count)
            {
                return string.Empty;
            }

            return Convert.ToString(row[indice] ?? string.Empty).Trim();
        }

        private bool TryParseDecimalFlexible(string input, out decimal value)
        {
            string limpio = (input ?? string.Empty).Trim();
            if (decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            limpio = limpio.Replace(".", string.Empty).Replace(",", ".");
            return decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private string EjecutarCargaIncremental(List<ProductoImportRow> filas)
        {
            using (var dc = new MonolitoDataContext())
            {
                dc.Connection.Open();
                using (var transaction = dc.Connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dc.Transaction = transaction;
                    try
                    {
                        var proveedores = dc.tbl_proveedor
                            .Where(p => p.prov_estado == 'A')
                            .ToList()
                            .ToDictionary(p => NormalizarClave(p.prov_nombre), p => p);

                        var productosExistentes = dc.tbl_producto
                            .Where(p => p.pro_estado == 'A')
                            .ToList()
                            .ToDictionary(p => NormalizarClave(p.pro_nombre), p => p);

                        var productosNuevos = new List<tbl_producto>();
                        var filasInsertadas = new List<Tuple<tbl_producto, ProductoImportRow>>();
                        var errores = new List<string>();
                        var nombresArchivo = new HashSet<string>();

                        foreach (var fila in filas)
                        {
                            string claveProveedor = NormalizarClave(fila.ProviderName);
                            if (!proveedores.TryGetValue(claveProveedor, out tbl_proveedor proveedor))
                            {
                                errores.Add($"Fila {fila.RowNumber}: proveedor \"{fila.ProviderName}\" no existe o está inactivo.");
                                continue;
                            }

                            string claveProducto = NormalizarClave(fila.ProductName);
                            if (productosExistentes.ContainsKey(claveProducto) || !nombresArchivo.Add(claveProducto))
                            {
                                errores.Add($"Fila {fila.RowNumber}: producto \"{fila.ProductName}\" ya existe.");
                                continue;
                            }

                            var producto = new tbl_producto
                            {
                                pro_nombre = fila.ProductName,
                                pro_cantidad = fila.Quantity,
                                pro_precio = fila.Price,
                                pro_estado = 'A',
                                prov_id = proveedor.prov_id
                            };

                            productosNuevos.Add(producto);
                            filasInsertadas.Add(Tuple.Create(producto, fila));
                            productosExistentes[claveProducto] = producto;
                        }

                        if (productosNuevos.Any())
                        {
                            dc.tbl_producto.InsertAllOnSubmit(productosNuevos);
                            dc.SubmitChanges();

                            var fotos = ConstruirFotosDesdeFilas(filasInsertadas);
                            if (fotos.Any())
                            {
                                dc.tbl_pro_fotos.InsertAllOnSubmit(fotos);
                                dc.SubmitChanges();
                            }
                        }

                        transaction.Commit();

                        string resumen = $"Carga incremental completada. Insertados: {productosNuevos.Count}.";
                        if (errores.Any())
                        {
                            resumen += $" Omitidos: {errores.Count}. {string.Join(" ", errores.Take(5))}";
                        }
                        return resumen;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dc.Connection.Close();
                    }
                }
            }
        }

        private string EjecutarSobrescrituraCompleta(List<ProductoImportRow> filas)
        {
            using (var dc = new MonolitoDataContext())
            {
                dc.Connection.Open();
                using (var transaction = dc.Connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dc.Transaction = transaction;
                    try
                    {
                        var proveedores = dc.tbl_proveedor
                            .Where(p => p.prov_estado == 'A')
                            .ToList()
                            .ToDictionary(p => NormalizarClave(p.prov_nombre), p => p);

                        var nombresDuplicados = filas
                            .GroupBy(f => NormalizarClave(f.ProductName))
                            .Where(g => g.Count() > 1)
                            .Select(g => g.First().ProductName)
                            .ToList();

                        if (nombresDuplicados.Any())
                        {
                            throw new InvalidOperationException("La carga contiene productos duplicados: " + string.Join(", ", nombresDuplicados));
                        }

                        foreach (var fila in filas)
                        {
                            if (!proveedores.ContainsKey(NormalizarClave(fila.ProviderName)))
                            {
                                throw new InvalidOperationException($"Fila {fila.RowNumber}: proveedor \"{fila.ProviderName}\" no existe o está inactivo.");
                            }
                        }

                        dc.ExecuteCommand("DELETE FROM dbo.tbl_pro_fotos");
                        dc.ExecuteCommand("DELETE FROM dbo.tbl_producto");
                        dc.ExecuteCommand("DBCC CHECKIDENT ('dbo.tbl_producto', RESEED, 0)");

                        var productos = filas.Select(f => new tbl_producto
                        {
                            pro_nombre = f.ProductName,
                            pro_cantidad = f.Quantity,
                            pro_precio = f.Price,
                            pro_estado = 'A',
                            prov_id = proveedores[NormalizarClave(f.ProviderName)].prov_id
                        }).ToList();

                        dc.tbl_producto.InsertAllOnSubmit(productos);
                        dc.SubmitChanges();

                        var fotos = ConstruirFotosDesdeFilas(productos.Zip(filas, Tuple.Create).ToList());
                        if (fotos.Any())
                        {
                            dc.tbl_pro_fotos.InsertAllOnSubmit(fotos);
                            dc.SubmitChanges();
                        }

                        transaction.Commit();
                        return $"Sobrescritura completa finalizada. Productos cargados: {productos.Count}. Referencias de foto registradas: {fotos.Count}.";
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dc.Connection.Close();
                    }
                }
            }
        }

        private List<tbl_pro_fotos> ConstruirFotosDesdeFilas(IEnumerable<Tuple<tbl_producto, ProductoImportRow>> registros)
        {
            var fotos = new List<tbl_pro_fotos>();
            foreach (var registro in registros)
            {
                foreach (string referencia in SepararReferenciasFoto(registro.Item2.ImageReference))
                {
                    fotos.Add(new tbl_pro_fotos
                    {
                        pro_id = registro.Item1.pro_id,
                        foto_ruta = NormalizarRutaFotoImportada(referencia),
                        foto_estado = 'A',
                        fecha_subida = DateTime.Now
                    });
                }
            }

            return fotos;
        }

        private IEnumerable<string> SepararReferenciasFoto(string valor)
        {
            return (valor ?? string.Empty)
                .Split(SeparadoresFotos, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private string NormalizarRutaFotoImportada(string ruta)
        {
            string limpia = (ruta ?? string.Empty).Trim().Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(limpia))
            {
                return string.Empty;
            }

            if (limpia.StartsWith("~/", StringComparison.Ordinal))
            {
                return limpia.Substring(2);
            }

            if (limpia.StartsWith("/", StringComparison.Ordinal))
            {
                return limpia.TrimStart('/');
            }

            if (!limpia.Contains("/"))
            {
                return "Uploads/Productos/" + limpia;
            }

            return limpia;
        }

        private string NormalizarClave(string valor)
        {
            return Regex.Replace((valor ?? string.Empty).Trim().ToUpperInvariant(), @"\s+", " ");
        }

        public string GenerarCarrusel(object proId, object fotosObj)
        {
            var fotos = (fotosObj as EntitySet<tbl_pro_fotos>)
                ?.Where(f => f != null &&
                             f.foto_estado == 'A' &&
                             !string.IsNullOrWhiteSpace(f.foto_ruta))
                .OrderByDescending(f => f.fecha_subida)
                .ToList();

            if (fotos == null || !fotos.Any())
            {
                return "<div class='no-foto'>" +
                       "<i class='fa-solid fa-camera' style='font-size:1.3rem;'></i>" +
                       "</div>";
            }

            string html = "<div class='carousel-cell'>";
            for (int i = 0; i < fotos.Count; i++)
            {
                string act = i == 0 ? " active" : "";
                string fotoUrl = Server.HtmlEncode(ResolverRutaFotoVisual(fotos[i].foto_ruta));
                html += $"<div class='slide{act}'>" +
                        $"<img src='{fotoUrl}' alt='Foto {i + 1}' onerror=\"this.src='https://placehold.co/110x80/ede6f8/7a4aaa?text=?'\"/>" +
                        "</div>";
            }

            if (fotos.Count > 1)
            {
                html += "<button type='button' class='prev'><i class='fa-solid fa-chevron-left'></i></button>";
                html += "<button type='button' class='next'><i class='fa-solid fa-chevron-right'></i></button>";
                html += "<div class='dots'>";
                for (int j = 0; j < fotos.Count; j++)
                {
                    html += $"<div class='dot{(j == 0 ? " on" : "")}'></div>";
                }
                html += "</div>";
            }

            html += "</div>";
            return html;
        }

        private string ResolverRutaFotoVisual(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return "https://placehold.co/110x80/ede6f8/7a4aaa?text=?";
            }

            string limpia = ruta.Trim();
            if (limpia.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                limpia.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return limpia;
            }

            return ResolveUrl("~/" + limpia.TrimStart('~', '/'));
        }

        private void LimpiarFormulario()
        {
            hfProdId.Value = "0";
            txtNombre.Text = "";
            txtCantidad.Text = "";
            txtPrecio.Text = "";
            ddlProveedor.SelectedIndex = 0;
            litFotosModalInfo.Text = "Puedes adjuntar hasta 4 fotos JPG o PNG. Se registrar&aacute;n en FotosProducto al guardar el producto.";
            litTituloModal.Text = "Nuevo Producto";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            string icon = exito ? "success" : "error";
            string title = exito ? "Éxito" : "Atención";
            string textoSeguro = (texto ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", " ")
                .Replace("\n", " ");
            string script = $"Swal.fire({{ title: '{title}', text: '{textoSeguro}', icon: '{icon}', confirmButtonColor: '#7a4aaa' }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "swal_msg", script, true);

            string css = exito ? "alert alert-success" : "alert alert-danger";
            litMensaje.Text = $"<div class='{css}'>{Server.HtmlEncode(texto)}</div>";
        }

        private sealed class ProductoImportRow
        {
            public int RowNumber { get; set; }
            public string ProductName { get; set; }
            public string ProviderName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public string ImageReference { get; set; }
        }
    }
}
