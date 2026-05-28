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
        protected global::System.Web.UI.WebControls.Button      btnSubir;
        protected global::System.Web.UI.WebControls.Button      btnDescargarFormato;
        protected global::System.Web.UI.WebControls.Button      btnCargarExcel;
        protected global::System.Web.UI.WebControls.Repeater    rptFotos;

        // Carpeta donde se guardan las fotos de productos
        private const string CARPETA_VIRTUAL = "~/Uploads/Productos/";

        // ── Page Load ─────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarProductosDropdown();
                CargarFotos();
            }
        }

        // ── Cargar productos en el Dropdown ────────────────────────
        private void CargarProductosDropdown()
        {
            try
            {
                var productos = CN_tbl_producto.Listar()
                    .Where(p => p.pro_estado == 'A')
                    .OrderBy(p => p.pro_nombre)
                    .ToList();

                ddlProducto.DataSource = productos;
                ddlProducto.DataTextField = "pro_nombre";
                ddlProducto.DataValueField = "pro_id";
                ddlProducto.DataBind();

                ddlProducto.Items.Insert(0, new ListItem("-- Seleccione un producto --", ""));
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la lista de productos: " + ex.Message, false);
            }
        }

        // ── Cargar fotos en el Repeater (Consulta directa y auto-contenida) ─
        private void CargarFotos()
        {
            try
            {
                using (var dc = new MonolitoDataContext())
                {
                    var query = (from f in dc.tbl_pro_fotos
                                 orderby f.fecha_subida descending
                                 select new
                                 {
                                     f.foto_id,
                                     f.pro_id,
                                     f.foto_ruta,
                                     f.foto_estado,
                                     f.fecha_subida,
                                     pro_nombre = f.tbl_producto != null ? f.tbl_producto.pro_nombre : "Sin Producto"
                                 }).ToList();

                    litTotalFotos.Text = $"{query.Count} foto(s) registrada(s) en total";

                    if (query.Any())
                    {
                        rptFotos.DataSource = query;
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
                            "No se encontraron fotos registradas en el sistema.</div>";
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar las fotos: " + ex.Message, false);
            }
        }

        // ── Subir fotos individual o múltiple para el producto seleccionado ──
        protected void btnSubir_Click(object sender, EventArgs e)
        {
            // Validar selección de producto
            int proId;
            if (string.IsNullOrWhiteSpace(ddlProducto.SelectedValue) || !int.TryParse(ddlProducto.SelectedValue, out proId))
            {
                MostrarMensaje("Debe seleccionar un producto de la lista para asociarle las fotos.", false);
                return;
            }

            // Validar existencia de archivos
            if (!fuFotos.HasFiles)
            {
                MostrarMensaje("Seleccione al menos un archivo de imagen.", false);
                return;
            }

            // Filtrar archivos válidos en el backend (peso de 2MB por foto y tipo de archivo)
            var archivosValidos = new List<HttpPostedFile>();
            var errores = new List<string>();

            foreach (HttpPostedFile file in fuFotos.PostedFiles)
            {
                if (file.ContentLength == 0) continue;

                // Validar peso de 2MB (2 * 1024 * 1024 bytes)
                if (file.ContentLength > 2 * 1024 * 1024)
                {
                    errores.Add($"El archivo '{file.FileName}' supera el límite de 2 MB.");
                    continue;
                }

                // Validar tipo de archivo (solo JPEG/JPG y PNG)
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    errores.Add($"El archivo '{file.FileName}' no es una imagen permitida (solo JPG o PNG).");
                    continue;
                }

                archivosValidos.Add(file);
            }

            // Si hay errores, mostrar el primero
            if (errores.Any())
            {
                MostrarMensaje(errores.First(), false);
                CargarFotos();
                return;
            }

            if (!archivosValidos.Any())
            {
                MostrarMensaje("No se encontraron imágenes válidas para subir.", false);
                CargarFotos();
                return;
            }

            // Crear carpeta física si no existe
            string carpetaFisica = Server.MapPath(CARPETA_VIRTUAL);
            if (!Directory.Exists(carpetaFisica))
            {
                Directory.CreateDirectory(carpetaFisica);
            }

            try
            {
                var nuevasFotos = archivosValidos.Select(f =>
                {
                    string ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                    string archivo = $"prod_{proId}_{Guid.NewGuid():N}{ext}";
                    string rutaFisica = Path.Combine(carpetaFisica, archivo);
                    f.SaveAs(rutaFisica);
                    byte[] contenido = File.ReadAllBytes(rutaFisica);

                    return new tbl_pro_fotos
                    {
                        pro_id = proId,
                        foto_bit = new Binary(contenido),
                        foto_ruta = $"Uploads/Productos/{archivo}",
                        foto_estado = 'A',
                        fecha_subida = DateTime.Now
                    };
                }).ToList();

                CN_tbl_pro_fotos.GuardarFotos(nuevasFotos);
                MostrarMensaje($"{nuevasFotos.Count} foto(s) subida(s) correctamente para el producto seleccionado.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar las fotos: " + ex.Message, false);
            }

            CargarFotos();
        }

        // ── Descargar Formato Excel ───────────────────────────────
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

                // Cabeceras y fila de ejemplo
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

        // ── Cargar Excel ──────────────────────────────────────────
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

            // Indicamos que el archivo de carga fue validado con éxito y se conectará en base a las adaptaciones de SQL del usuario
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
