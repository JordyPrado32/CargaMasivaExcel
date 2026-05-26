using Capa_Datos;
using Capa_Negocios;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Linq;

namespace Monolito4bm
{
    public partial class FotosProducto : System.Web.UI.Page
    {
        // ── Controles declarados manualmente (evita errores de designer) ──
        protected global::System.Web.UI.WebControls.HiddenField hfProId;
        protected global::System.Web.UI.WebControls.Literal     litNombreProducto;
        protected global::System.Web.UI.WebControls.Literal     litMensaje;
        protected global::System.Web.UI.WebControls.Literal     litAviso;
        protected global::System.Web.UI.WebControls.Literal     litTotalFotos;
        protected global::System.Web.UI.WebControls.Literal     litSinFotos;
        protected global::System.Web.UI.WebControls.FileUpload  fuFotos;
        protected global::System.Web.UI.WebControls.Button      btnSubir;
        protected global::System.Web.UI.WebControls.Button      btnCancelar;
        protected global::System.Web.UI.WebControls.Repeater    rptFotos;

        // Carpeta donde se guardan las fotos de productos
        private const string CARPETA_VIRTUAL = "~/Uploads/Productos/";

        // ── Page Load ─────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            int proId = 0;
            if (!int.TryParse(Request.QueryString["id"], out proId) || proId == 0)
            {
                Response.Redirect("Productos.aspx");
                return;
            }

            hfProId.Value = proId.ToString();

            if (!IsPostBack)
            {
                CargarNombreProducto(proId);
                CargarFotos(proId);
            }
        }

        // ── Nombre del producto ───────────────────────────────────
        private void CargarNombreProducto(int proId)
        {
            var prod = CN_tbl_producto.BuscarPorId(proId);
            litNombreProducto.Text = prod != null
                ? Server.HtmlEncode(prod.pro_nombre)
                : "Producto #" + proId;
        }

        // ── Cargar fotos en el Repeater ───────────────────────────
        private void CargarFotos(int proId)
        {
            // Fotos con datos del producto incluidos (para mostrar nombre)
            var fotos = CN_tbl_pro_fotos.ObtenerConProducto(proId);

            int total = fotos.Count;
            litTotalFotos.Text = $"{total} / 4 foto(s)";

            // Aviso de límite
            litAviso.Text = total >= 4
                ? "<div class='limite-aviso'>" +
                  "<i class='fa-solid fa-triangle-exclamation'></i> " +
                  "L&iacute;mite de 4 fotos alcanzado. Elimina una para poder subir m&aacute;s.</div>"
                : string.Empty;

            if (fotos.Any())
            {
                rptFotos.DataSource = fotos;
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
                    "Este producto a&uacute;n no tiene fotos.</div>";
            }
        }

        // ── Subir fotos ───────────────────────────────────────────
        protected void btnSubir_Click(object sender, EventArgs e)
        {
            int proId = int.Parse(hfProId.Value);
            int yaExisten = CN_tbl_pro_fotos.Contar(proId);
            int disponibles = 4 - yaExisten;

            if (disponibles <= 0)
            {
                MostrarMensaje("Ya tiene 4 fotos. Elimina alguna antes de subir.", false);
                CargarFotos(proId);
                return;
            }

            if (!fuFotos.HasFiles)
            {
                MostrarMensaje("Selecciona al menos un archivo.", false);
                CargarFotos(proId);
                return;
            }

            var validos = fuFotos.PostedFiles
                .Where(f => f.ContentLength > 0
                         && f.ContentLength <= 2 * 1024 * 1024
                         && (f.ContentType == "image/jpeg" || f.ContentType == "image/png"))
                .Take(disponibles)
                .ToList();

            if (!validos.Any())
            {
                MostrarMensaje("No hay archivos válidos. Solo JPG o PNG de máx. 2 MB.", false);
                CargarFotos(proId);
                return;
            }

            // Crear carpeta física si no existe
            string carpetaFisica = Server.MapPath(CARPETA_VIRTUAL);
            if (!Directory.Exists(carpetaFisica))
                Directory.CreateDirectory(carpetaFisica);

            try
            {
                var nuevas = validos.Select(f =>
                {
                    string ext = f.ContentType == "image/png" ? ".png" : ".jpg";
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

                CN_tbl_pro_fotos.GuardarFotos(nuevas);
                MostrarMensaje($"{nuevas.Count} foto(s) subida(s) correctamente.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al subir: " + ex.Message, false);
            }

            CargarFotos(proId);
        }

        // ── Volver a productos ────────────────────────────────────
        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("Productos.aspx");
        }

        // ── Comandos del Repeater ─────────────────────────────────
        protected void rptFotos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int fotoId = int.Parse(e.CommandArgument.ToString());
            int proId = int.Parse(hfProId.Value);

            switch (e.CommandName)
            {
                // ── Desactivación lógica ──────────────────────────
                case "Desactivar":
                    try
                    {
                        CN_tbl_pro_fotos.CambiarEstado(fotoId, 'I');
                        MostrarMensaje("Foto desactivada.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, false); }
                    break;

                // ── Reactivación lógica ───────────────────────────
                case "Reactivar":
                    try
                    {
                        CN_tbl_pro_fotos.CambiarEstado(fotoId, 'A');
                        MostrarMensaje("Foto reactivada.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, false); }
                    break;

                // ── Eliminación física permanente ─────────────────
                case "ElimFis":
                    try
                    {
                        string ruta = CN_tbl_pro_fotos.EliminarFisico(fotoId);
                        if (!string.IsNullOrEmpty(ruta))
                        {
                            string rutaFis = Server.MapPath("~/" + ruta);
                            if (File.Exists(rutaFis))
                                File.Delete(rutaFis);
                        }
                        MostrarMensaje("Foto eliminada permanentemente.", true);
                    }
                    catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, false); }
                    break;
            }

            CargarFotos(proId);
        }

        // ── Helper mensajes ───────────────────────────────────────
        private void MostrarMensaje(string texto, bool exito)
        {
            string icon = exito ? "success" : "error";
            string title = exito ? "¡Éxito!" : "¡Atención!";
            string script = $"Swal.fire({{ title: '{title}', text: '{texto}', icon: '{icon}', confirmButtonColor: '#7a4aaa' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "swal_msg_fotos", script, true);

            // litMensaje.Text = ""; // Opcional
        }
    }
}
