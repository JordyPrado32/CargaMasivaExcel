using Capa_Datos;
using Capa_Negocios;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Linq;
using System.Collections.Generic;

namespace Monolito4bm
{
    public partial class FotosProductosGeneral : System.Web.UI.Page
    {
        // ── Controles declarados manualmente (evita errores de designer) ──
        protected global::System.Web.UI.WebControls.Literal     litMensaje;
        protected global::System.Web.UI.WebControls.Literal     litTotalFotos;
        protected global::System.Web.UI.WebControls.Literal     litSinFotos;
        protected global::System.Web.UI.WebControls.FileUpload  fuFotos;
        protected global::System.Web.UI.WebControls.FileUpload  fuExcel;
        protected global::System.Web.UI.WebControls.DropDownList ddlProducto;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroProducto;
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;
        protected global::System.Web.UI.WebControls.TextBox      txtBuscar;
        protected global::System.Web.UI.WebControls.TextBox      txtFechaDesde;
        protected global::System.Web.UI.WebControls.TextBox      txtFechaHasta;
        protected global::System.Web.UI.WebControls.Button      btnSubir;
        protected global::System.Web.UI.WebControls.Button      btnPrevisualizar;
        protected global::System.Web.UI.WebControls.Button      btnDescargarFormato;
        protected global::System.Web.UI.WebControls.Button      btnCargarExcel;
        protected global::System.Web.UI.WebControls.LinkButton   btnLimpiarFiltros;
        protected global::System.Web.UI.WebControls.Repeater    rptFotos;
        protected global::System.Web.UI.WebControls.Repeater    rptFotosPreview;
        protected global::System.Web.UI.WebControls.Literal     lblFotosPreviewInfo;
        protected global::System.Web.UI.WebControls.HiddenField hfFiltrosAbiertos;

        private const string CARPETA_VIRTUAL = "~/Uploads/Productos/";
        private const string SessionFotosKey = "GeneralFotosPreview";

        // Clase para representar la foto cargada temporalmente en memoria
        [Serializable]
        private class FotoTemporal
        {
            public string Id { get; set; }
            public string NombreArchivo { get; set; }
            public string ContentType { get; set; }
            public byte[] Contenido { get; set; }

            public string PreviewUrl
            {
                get { return "data:" + ContentType + ";base64," + Convert.ToBase64String(Contenido); }
            }
        }

        // ── Page Load ─────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarProductosDropdowns();
                CargarFotos();
                LimpiarFotosTemporales();
                BindFotosPreview();
            }
        }

        // ── Cargar productos en los Dropdowns (Subida y Filtro) ──────
        private void CargarProductosDropdowns()
        {
            try
            {
                var productos = CN_tbl_producto.Listar()
                    .Where(p => p.pro_estado == 'A')
                    .OrderBy(p => p.pro_nombre)
                    .ToList();

                // 1. Dropdown de subida individual/múltiple
                ddlProducto.DataSource = productos;
                ddlProducto.DataTextField = "pro_nombre";
                ddlProducto.DataValueField = "pro_id";
                ddlProducto.DataBind();
                ddlProducto.Items.Insert(0, new ListItem("-- Seleccione un producto --", ""));

                // 2. Dropdown de filtros avanzados
                ddlFiltroProducto.DataSource = productos;
                ddlFiltroProducto.DataTextField = "pro_nombre";
                ddlFiltroProducto.DataValueField = "pro_id";
                ddlFiltroProducto.DataBind();
                ddlFiltroProducto.Items.Insert(0, new ListItem("Todos los productos", ""));
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la lista de productos: " + ex.Message, false);
            }
        }

        // ── Cargar fotos en el Repeater con Filtros Aplicados ─────────
        private void CargarFotos()
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    // Consulta base sobre tbl_pro_fotos
                    var query = from f in dc.tbl_pro_fotos
                                select f;

                    // 1. Filtro por búsqueda predictiva (Búsqueda por nombre del producto o ruta)
                    string busqueda = txtBuscar.Text.Trim();
                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        query = query.Where(f => f.tbl_producto.pro_nombre.Contains(busqueda) || f.foto_ruta.Contains(busqueda));
                    }

                    // 2. Filtro por producto seleccionado
                    int filtroProId;
                    if (int.TryParse(ddlFiltroProducto.SelectedValue, out filtroProId) && filtroProId > 0)
                    {
                        query = query.Where(f => f.pro_id == filtroProId);
                    }

                    // 3. Filtro por estado de foto
                    string filtroEstado = ddlFiltroEstado.SelectedValue;
                    if (!string.IsNullOrEmpty(filtroEstado))
                    {
                        char est = filtroEstado[0];
                        query = query.Where(f => f.foto_estado == est);
                    }

                    // 4. Filtro por rango de fecha de subida (Desde)
                    DateTime fechaDesde;
                    if (DateTime.TryParse(txtFechaDesde.Text, out fechaDesde))
                    {
                        query = query.Where(f => f.fecha_subida >= fechaDesde.Date);
                    }

                    // 5. Filtro por rango de fecha de subida (Hasta)
                    DateTime fechaHasta;
                    if (DateTime.TryParse(txtFechaHasta.Text, out fechaHasta))
                    {
                        DateTime limiteHasta = fechaHasta.Date.AddDays(1);
                        query = query.Where(f => f.fecha_subida < limiteHasta);
                    }

                    // Proyectamos a objetos anónimos ordenados de forma descendente por fecha de subida
                    var listado = query
                        .OrderByDescending(f => f.fecha_subida)
                        .Select(f => new
                        {
                            f.foto_id,
                            f.pro_id,
                            f.foto_ruta,
                            f.foto_estado,
                            f.fecha_subida,
                            pro_nombre = f.tbl_producto != null ? f.tbl_producto.pro_nombre : "Sin Producto"
                        })
                        .ToList();

                    litTotalFotos.Text = $"{listado.Count} foto(s) encontrada(s)";

                    if (listado.Any())
                    {
                        rptFotos.DataSource = listado;
                        rptFotos.DataBind();
                        litSinFotos.Text = string.Empty;
                    }
                    else
                    {
                        rptFotos.DataSource = null;
                        rptFotos.DataBind();
                        litSinFotos.Text =
                            "<div class='empty-state'>" +
                            "<i class='fa-solid fa-camera-slash'></i>" +
                            "No se encontraron fotos con los filtros aplicados.</div>";
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al filtrar las fotos: " + ex.Message, false);
            }
        }

        // ── Gestión de Fotos Temporales en Session ─────────────────────
        private List<FotoTemporal> ObtenerFotosTemporales()
        {
            return Session[SessionFotosKey] as List<FotoTemporal> ?? new List<FotoTemporal>();
        }

        private void GuardarFotosTemporales(List<FotoTemporal> fotos)
        {
            Session[SessionFotosKey] = fotos;
        }

        private void LimpiarFotosTemporales()
        {
            Session.Remove(SessionFotosKey);
        }

        private void BindFotosPreview()
        {
            var fotos = ObtenerFotosTemporales();
            rptFotosPreview.DataSource = fotos;
            rptFotosPreview.DataBind();

            if (fotos.Count > 0)
            {
                lblFotosPreviewInfo.Text = $"Fotos preparadas en la previsualización: {fotos.Count}. Presione 'Subir fotos' para guardarlas en la base de datos.";
            }
            else
            {
                lblFotosPreviewInfo.Text = "No hay fotos en la previsualización temporal.";
            }
        }

        // ── Agregar archivos cargados a la previsualización temporal (C#) ──
        protected void btnPrevisualizar_Click(object sender, EventArgs e)
        {
            int proId;
            if (string.IsNullOrWhiteSpace(ddlProducto.SelectedValue) || !int.TryParse(ddlProducto.SelectedValue, out proId))
            {
                MostrarMensaje("Debe seleccionar un producto antes de agregar fotos a la previsualización.", false);
                return;
            }

            if (!fuFotos.HasFiles)
            {
                MostrarMensaje("Seleccione al menos un archivo de imagen para previsualizar.", false);
                return;
            }

            var fotosActuales = ObtenerFotosTemporales();
            var nuevasFotos = new List<FotoTemporal>();

            foreach (HttpPostedFile file in fuFotos.PostedFiles)
            {
                if (file.ContentLength == 0) continue;

                // Validar peso de 2MB
                if (file.ContentLength > 2 * 1024 * 1024)
                {
                    MostrarMensaje($"El archivo '{file.FileName}' supera el límite de 2 MB.", false);
                    return;
                }

                // Validar extensión
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    MostrarMensaje($"El archivo '{file.FileName}' no es JPG ni PNG.", false);
                    return;
                }

                using (var reader = new BinaryReader(file.InputStream))
                {
                    byte[] contenido = reader.ReadBytes(file.ContentLength);
                    nuevasFotos.Add(new FotoTemporal
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        NombreArchivo = Path.GetFileName(file.FileName),
                        ContentType = file.ContentType,
                        Contenido = contenido
                    });
                }
            }

            fotosActuales.AddRange(nuevasFotos);
            GuardarFotosTemporales(fotosActuales);
            BindFotosPreview();
        }

        // ── Handler para comandos de la previsualización (C# - Botón X) ──
        protected void rptFotosPreview_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Eliminar")
            {
                string id = e.CommandArgument.ToString();
                var fotos = ObtenerFotosTemporales();
                var fotoAEliminar = fotos.FirstOrDefault(f => f.Id == id);
                if (fotoAEliminar != null)
                {
                    fotos.Remove(fotoAEliminar);
                    GuardarFotosTemporales(fotos);
                }
                BindFotosPreview();
            }
        }

        // ── Subir fotos definitivas desde la previsualización temporal ──
        protected void btnSubir_Click(object sender, EventArgs e)
        {
            int proId;
            if (string.IsNullOrWhiteSpace(ddlProducto.SelectedValue) || !int.TryParse(ddlProducto.SelectedValue, out proId))
            {
                MostrarMensaje("Debe seleccionar un producto de la lista para asociarle las fotos.", false);
                return;
            }

            var fotosTemporales = ObtenerFotosTemporales();
            if (!fotosTemporales.Any())
            {
                MostrarMensaje("No hay fotos en la previsualización temporal. Cargue imágenes y presione 'Agregar a Previsualización'.", false);
                return;
            }

            string carpetaFisica = Server.MapPath(CARPETA_VIRTUAL);
            if (!Directory.Exists(carpetaFisica))
            {
                Directory.CreateDirectory(carpetaFisica);
            }

            try
            {
                var nuevasFotos = fotosTemporales.Select(f =>
                {
                    string ext = Path.GetExtension(f.NombreArchivo).ToLowerInvariant();
                    string archivo = $"prod_{proId}_{Guid.NewGuid():N}{ext}";
                    string rutaFisica = Path.Combine(carpetaFisica, archivo);
                    
                    File.WriteAllBytes(rutaFisica, f.Contenido);

                    return new tbl_pro_fotos
                    {
                        pro_id = proId,
                        foto_bit = new Binary(f.Contenido),
                        foto_ruta = $"Uploads/Productos/{archivo}",
                        foto_estado = 'A',
                        fecha_subida = DateTime.Now
                    };
                }).ToList();

                CN_tbl_pro_fotos.GuardarFotos(nuevasFotos);
                
                LimpiarFotosTemporales();
                BindFotosPreview();
                ddlProducto.SelectedIndex = 0;

                MostrarMensaje($"{nuevasFotos.Count} foto(s) guardada(s) correctamente en la base de datos.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar las fotos en la base de datos: " + ex.Message, false);
            }

            CargarFotos();
        }

        // ── Handler para cambios en filtros ──────────────────────────
        protected void Filtros_Changed(object sender, EventArgs e)
        {
            CargarFotos();
        }

        // ── Handler para limpiar filtros ─────────────────────────────
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            ddlFiltroProducto.SelectedIndex = 0;
            ddlFiltroEstado.SelectedIndex = 0;
            txtFechaDesde.Text = string.Empty;
            txtFechaHasta.Text = string.Empty;
            hfFiltrosAbiertos.Value = "0";

            CargarFotos();
        }

        // ── Descargar Formato Excel (Plantilla CSV) ──────────────────
        protected void btnDescargarFormato_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=PlantillaCargaFotos.csv");
                Response.Charset = "UTF-8";
                Response.ContentType = "text/csv";
                Response.ContentEncoding = System.Text.Encoding.UTF8;

                Response.Write("ID_Producto,Nombre_Archivo\n");
                Response.Write("1,nombre_imagen_ejemplo.jpg\n");
                Response.Write("2,otra_imagen_ejemplo.png\n");

                Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Esperado en Response.End()
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al descargar el formato: " + ex.Message, false);
            }
        }

        // ── Cargar Excel (Validaciones) ─────────────────────────────
        protected void btnCargarExcel_Click(object sender, EventArgs e)
        {
            if (!fuExcel.HasFile)
            {
                MostrarMensaje("Debe seleccionar un archivo de Excel para cargar.", false);
                return;
            }

            string ext = Path.GetExtension(fuExcel.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls" && ext != ".csv")
            {
                MostrarMensaje("El archivo seleccionado debe ser un archivo de Excel (.xlsx, .xls) o CSV.", false);
                return;
            }

            if (fuExcel.PostedFile.ContentLength > 2 * 1024 * 1024)
            {
                MostrarMensaje("El archivo de Excel supera el límite de peso de 2 MB.", false);
                return;
            }

            string script = $"Swal.fire({{ " +
                           $"title: 'Excel Validado con éxito', " +
                           $"text: 'El archivo \"{fuExcel.FileName}\" fue cargado y validado. Conectando con las modificaciones de SQL de la base de datos...', " +
                           $"icon: 'success', " +
                           $"confirmButtonColor: '#7a4aaa' " +
                           $"}});";
            ClientScript.RegisterStartupScript(this.GetType(), "excel_msg", script, true);
        }

        // ── Acciones de la tabla (Repeater) ───────────────────────
        protected void rptFotos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int fotoId = int.Parse(e.CommandArgument.ToString());

            switch (e.CommandName)
            {
                case "Desactivar":
                    try
                    {
                        CN_tbl_pro_fotos.CambiarEstado(fotoId, 'I');
                        MostrarMensaje("Foto desactivada con éxito.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error al desactivar la foto: " + ex.Message, false); }
                    break;

                case "Reactivar":
                    try
                    {
                        CN_tbl_pro_fotos.CambiarEstado(fotoId, 'A');
                        MostrarMensaje("Foto reactivada con éxito.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error al reactivar la foto: " + ex.Message, false); }
                    break;

                case "ElimFis":
                    try
                    {
                        string ruta = CN_tbl_pro_fotos.EliminarFisico(fotoId);
                        if (!string.IsNullOrEmpty(ruta))
                        {
                            string rutaFisica = Server.MapPath("~/" + ruta);
                            if (File.Exists(rutaFisica))
                            {
                                File.Delete(rutaFisica);
                            }
                        }
                        MostrarMensaje("La foto se eliminó físicamente de forma permanente.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error al eliminar físicamente la foto: " + ex.Message, false); }
                    break;
            }

            CargarFotos();
        }

        // ── Helper Mensajes SweetAlert ───────────────────────────
        private void MostrarMensaje(string texto, bool exito)
        {
            string icon = exito ? "success" : "error";
            string title = exito ? "¡Éxito!" : "¡Atención!";
            string script = $"Swal.fire({{ title: '{title}', text: '{texto}', icon: '{icon}', confirmButtonColor: '#7a4aaa' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "swal_msg_general_fotos", script, true);
        }
    }
}
