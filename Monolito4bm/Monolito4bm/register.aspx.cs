using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Linq;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class register : System.Web.UI.Page
    {
        private const string SessionFotosKey = "RegisterFotosPreview";
        private const int MinFotos = 3;
        private const int MaxFotos = 5;
        private const int MaxPesoFotoBytes = 2 * 1024 * 1024;
        private const string CarpetaFotosRegistroVirtual = "~/Uploads/Usuarios/";
        private static readonly string[] ParticulasCompuestas =
        {
            "de", "del", "la", "las", "los", "san", "santa", "da", "das", "do", "dos", "van", "von"
        };

        [Serializable]
        private class FotoTemporal
        {
            public string Id { get; set; }
            public string NombreArchivo { get; set; }
            public string ContentType { get; set; }
            public string Extension { get; set; }
            public byte[] Contenido { get; set; }

            public string PreviewUrl
            {
                get { return "data:" + ContentType + ";base64," + Convert.ToBase64String(Contenido); }
            }
        }

        private class PartesNombre
        {
            public string PrimeraParte { get; set; }
            public string SegundaParte { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // TRUCO MAESTRO: Esto fuerza a ASP.NET a retener los valores en los PostBacks (del UpdatePanel o Botones)
            // aunque el usuario no pueda editarlos manualmente.
            txtEdad.Attributes.Add("readonly", "readonly");
            txtNickname.Attributes.Add("readonly", "readonly");

            if (!IsPostBack)
            {
                LimpiarFotosTemporales();
            }

            BindFotosPreview();
        }

        protected void btnPrevisualizar_Click(object sender, EventArgs e)
        {
            try
            {
                GuardarFotosTemporalesDesdeUpload();
                BindFotosPreview();
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("warning", "Atención", ex.Message);
            }
        }

        protected void rptFotosPreview_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Eliminar") return;

            List<FotoTemporal> fotos = ObtenerFotosTemporales();
            FotoTemporal fotoEliminar = fotos.FirstOrDefault(f => f.Id == e.CommandArgument.ToString());

            if (fotoEliminar == null) return;

            fotos.Remove(fotoEliminar);
            GuardarFotosTemporales(fotos);
            BindFotosPreview();
        }
        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<FotoTemporal> fotosTemporales = ObtenerFotosTemporales();
                if (fotosTemporales.Count == 0 && fuFotos.HasFiles)
                {
                    GuardarFotosTemporalesDesdeUpload();
                    fotosTemporales = ObtenerFotosTemporales();
                }

                if (fotosTemporales.Count < MinFotos || fotosTemporales.Count > MaxFotos)
                {
                    throw new Exception($"Debe mantener entre {MinFotos} y {MaxFotos} fotografías antes de finalizar el registro.");
                }

                PartesNombre nombres = SepararEnDosPartes(txtNombres.Text, "nombres");
                PartesNombre apellidos = SepararApellidosEstricto(txtApellidos.Text);

                // ARREGLO 1: Especificamos explícitamente que use la clase de Negocios
                Capa_Negocios.tbl_usuario nuevoUsuario = new Capa_Negocios.tbl_usuario();

                nuevoUsuario.usu_cedula = txtCedula.Text.Trim();
                nuevoUsuario.correo_electronico = txtCorreo.Text.Trim();
                nuevoUsuario.numero_celular = txtCelular.Text.Trim();
                nuevoUsuario.rol_id = 2;
                DateTime fechaNacimiento;
                // C# ahora espera el formato nativo de HTML5 con guiones
                if (!DateTime.TryParseExact(
                    txtFechaNac.Text.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fechaNacimiento))
                {
                    throw new Exception("Fecha de nacimiento no válida.");
                }
                nuevoUsuario.fecha_nacimiento = fechaNacimiento;

                int calculoEdad = DateTime.Today.Year - fechaNacimiento.Year;
                if (fechaNacimiento.Date > DateTime.Today.AddYears(-calculoEdad))
                {
                    calculoEdad--;
                }
                if (calculoEdad < 18 || calculoEdad > 120)
                {
                    throw new Exception("La edad permitida para registrarse es de 18 a 120 años.");
                }

                nuevoUsuario.edad = calculoEdad;
                int idUsuarioGenerado = nuevoUsuario.Registrarse(
                    txtPass.Text,
                    nombres.PrimeraParte,
                    nombres.SegundaParte,
                    apellidos.PrimeraParte,
                    apellidos.SegundaParte
                );

                List<Capa_Negocios.tbl_foto> listaFotos = new List<Capa_Negocios.tbl_foto>();
                foreach (FotoTemporal fotoTemporal in fotosTemporales)
                {
                    string rutaFoto = GuardarFotoRegistroEnDisco(idUsuarioGenerado, fotoTemporal);
                    listaFotos.Add(new Capa_Negocios.tbl_foto
                    {
                        nombre_archivo = fotoTemporal.NombreArchivo,
                        content_type = fotoTemporal.ContentType,
                        foto = fotoTemporal.Contenido,
                        foto_ruta = rutaFoto
                    });
                }

                Capa_Negocios.tbl_foto negocioFotos = new Capa_Negocios.tbl_foto();
                negocioFotos.RegistrarFotosValidadas(idUsuarioGenerado, listaFotos);

                txtNickname.Text = nuevoUsuario.usu_nickname;

                LimpiarFotosTemporales();
                BindFotosPreview();

                InjectarSweetAlertConRedireccion("success", "Registro exitoso", $"Usuario creado.<br>Tu Nickname es: <b>{nuevoUsuario.usu_nickname}</b>");
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                // 2627 y 2601 son los códigos oficiales de SQL Server para datos duplicados
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    // Revisamos si el error fue por la cédula o por el correo
                    if (sqlEx.Message.ToLower().Contains("cedula"))
                    {
                        InjectarSweetAlert("warning", "Cédula Repetida", "La cédula que intentas registrar ya existe en el sistema.");
                    }
                    else if (sqlEx.Message.ToLower().Contains("correo"))
                    {
                        InjectarSweetAlert("warning", "Correo Repetido", "Este correo electrónico ya está en uso por otro usuario.");
                    }
                    else
                    {
                        InjectarSweetAlert("warning", "Dato Duplicado", "La cédula o el correo ya se encuentran registrados.");
                    }
                }
                else
                {
                    // Si es otro error de SQL, lo mostramos normal
                    InjectarSweetAlert("error", "Error de Base de Datos", sqlEx.Message);
                }
            }
            catch (Exception ex)
            {
                // Para cualquier otro error general (como campos vacíos o edad incorrecta)
                InjectarSweetAlert("error", "Atención", ex.Message);
            }
        } // <--- Aquí termina tu método btnRegistrar_Click
        // Lógica estricta de 2 apellidos para el Backend
        private PartesNombre SepararApellidosEstricto(string valorOriginal)
        {
            string valor = NormalizarEspacios(valorOriginal);
            if (string.IsNullOrWhiteSpace(valor)) throw new Exception("El campo de apellidos es obligatorio.");

            List<string> grupos = AgruparPartes(valor);
            if (grupos.Count != 2) throw new Exception("Debe ingresar exactamente dos apellidos válidos.");

            return new PartesNombre
            {
                PrimeraParte = grupos[0],
                SegundaParte = grupos[1]
            };
        }

        private PartesNombre SepararEnDosPartes(string valorOriginal, string etiquetaCampo)
        {
            string valor = NormalizarEspacios(valorOriginal);
            if (string.IsNullOrWhiteSpace(valor)) throw new Exception("El campo de " + etiquetaCampo + " es obligatorio.");

            List<string> grupos = AgruparPartes(valor);
            if (grupos.Count == 0) throw new Exception("El campo de " + etiquetaCampo + " no contiene valores válidos.");
            if (grupos.Count == 1) return new PartesNombre { PrimeraParte = grupos[0], SegundaParte = grupos[0] };
            if (grupos.Count == 2) return new PartesNombre { PrimeraParte = grupos[0], SegundaParte = grupos[1] };
            return new PartesNombre { PrimeraParte = grupos[0], SegundaParte = string.Join(" ", grupos.Skip(1)) };
        }

        private List<string> AgruparPartes(string valor)
        {
            string[] palabras = valor.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> grupos = new List<string>();
            List<string> acumulado = new List<string>();

            foreach (string palabra in palabras)
            {
                string actual = palabra.Trim();
                if (string.IsNullOrWhiteSpace(actual)) continue;

                acumulado.Add(actual);
                bool esParticula = ParticulasCompuestas.Contains(actual.ToLowerInvariant());

                if (!esParticula)
                {
                    grupos.Add(string.Join(" ", acumulado));
                    acumulado.Clear();
                }
            }

            if (acumulado.Count > 0)
            {
                if (grupos.Count == 0) grupos.Add(string.Join(" ", acumulado));
                else grupos[grupos.Count - 1] = grupos[grupos.Count - 1] + " " + string.Join(" ", acumulado);
            }

            return grupos.Select(NormalizarEspacios).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        private string NormalizarEspacios(string valor)
        {
            return string.Join(" ", (valor ?? string.Empty).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void GuardarFotosTemporalesDesdeUpload()
        {
            if (!fuFotos.HasFiles) throw new Exception("Seleccione al menos una imagen para agregar a la previsualización.");

            List<FotoTemporal> fotosActuales = ObtenerFotosTemporales();
            int capacidadDisponible = MaxFotos - fotosActuales.Count;

            if (capacidadDisponible <= 0) throw new Exception("Ya alcanzó el máximo de 5 imágenes.");
            if (fuFotos.PostedFiles.Count > capacidadDisponible) throw new Exception("Solo puede agregar " + capacidadDisponible + " imagen(es) más.");

            List<FotoTemporal> nuevasFotos = new List<FotoTemporal>();

            foreach (var file in fuFotos.PostedFiles)
            {
                if (file == null || file.ContentLength == 0) continue;
                if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("El archivo " + file.FileName + " no es una imagen válida.");
                }
                if (file.ContentLength > MaxPesoFotoBytes) throw new Exception("La imagen " + file.FileName + " supera los 2MB permitidos.");

                using (BinaryReader br = new BinaryReader(file.InputStream))
                {
                    byte[] contenido = br.ReadBytes(file.ContentLength);
                    nuevasFotos.Add(new FotoTemporal
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        NombreArchivo = Path.GetFileName(file.FileName),
                        ContentType = file.ContentType,
                        Extension = ObtenerExtensionSegura(file.FileName, file.ContentType),
                        Contenido = contenido
                    });
                }
            }

            if (nuevasFotos.Count == 0) throw new Exception("No se encontraron imágenes válidas para cargar.");

            fotosActuales.AddRange(nuevasFotos);
            GuardarFotosTemporales(fotosActuales);
        }

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

        private string GuardarFotoRegistroEnDisco(int usuarioId, FotoTemporal foto)
        {
            string carpetaFisica = Server.MapPath(CarpetaFotosRegistroVirtual);
            if (!Directory.Exists(carpetaFisica))
            {
                Directory.CreateDirectory(carpetaFisica);
            }

            string extension = string.IsNullOrWhiteSpace(foto.Extension)
                ? ObtenerExtensionSegura(foto.NombreArchivo, foto.ContentType)
                : foto.Extension;
            string nombreArchivo = $"usr_{usuarioId}_{Guid.NewGuid():N}{extension}";
            string rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);

            File.WriteAllBytes(rutaFisica, foto.Contenido);

            return "Uploads/Usuarios/" + nombreArchivo;
        }

        private static string ObtenerExtensionSegura(string nombreArchivo, string contentType)
        {
            string extension = Path.GetExtension(nombreArchivo ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                return extension.ToLowerInvariant();
            }

            if (string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return ".png";
            }

            if (string.Equals(contentType, "image/gif", StringComparison.OrdinalIgnoreCase))
            {
                return ".gif";
            }

            return ".jpg";
        }

        private void BindFotosPreview()
        {
            List<FotoTemporal> fotos = ObtenerFotosTemporales();
            pnlPreview.Visible = fotos.Count > 0;
            rptFotosPreview.DataSource = fotos;
            rptFotosPreview.DataBind();

            if (fotos.Count > 0)
            {
                string estado = fotos.Count >= MinFotos
                    ? "Ya puede finalizar el registro."
                    : "Necesita al menos " + (MinFotos - fotos.Count) + " imagen(es) más para registrar.";
                lblFotosInfo.Text = "Imágenes cargadas: " + fotos.Count + " de " + MaxFotos + ". " + estado;
            }
            else
            {
                lblFotosInfo.Text = string.Empty;
            }
        }
        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            // 1. Limpiamos ABSOLUTAMENTE todo lo que pueda romper el JavaScript
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");

            // 2. Construimos la alerta en una sola línea segura
            string script = $"Swal.fire('{titulo}', '{msjLimpio}', '{tipo}');";

            // 3. Inyectamos obligando a ASP.NET a poner las etiquetas <script>
            ScriptManager.RegisterStartupScript(this, GetType(), "SwalError", script, true);
        }
        private void InjectarSweetAlertConRedireccion(string tipo, string titulo, string mensaje)
        {
            string msjLimpio = mensaje.Replace("'", "").Replace("\"", "").Replace("\r", "").Replace("\n", " - ");

            // ¡AQUÍ ESTÁ EL CAMBIO! Reemplazamos 'login.aspx' por 'Default.aspx'
            string script = $"Swal.fire('{titulo}', '{msjLimpio}', '{tipo}').then(function() {{ window.location = 'Default.aspx'; }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "SwalRedirect", script, true);
        }
    }
}
