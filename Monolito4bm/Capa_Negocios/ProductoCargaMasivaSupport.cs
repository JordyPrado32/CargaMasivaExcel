using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Capa_Negocios
{
    [Serializable]
    public sealed class ProductoCargaFila
    {
        public int NumeroFilaArchivo { get; set; }
        public int? ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int? ProveedorId { get; set; }
        public char EstadoProducto { get; set; }
    }

    public sealed class ResultadoCargaProductos
    {
        public int FilasProcesadas { get; set; }
        public int Insertados { get; set; }
        public int Actualizados { get; set; }
        public int ProductosSinProveedor { get; set; }
        public int FotosEliminadas { get; set; }
    }

    internal sealed class ProductoCargaFilaNormalizada
    {
        public int NumeroFilaArchivo { get; set; }
        public int? ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string NombreNormalizado { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int? ProveedorId { get; set; }
        public char EstadoProducto { get; set; }
    }

    internal static class ProductoCargaMasivaParser
    {
        private static readonly string[] HeadersId = { "id", "proid", "productoid", "idproducto", "codigo", "codigoproducto" };
        private static readonly string[] HeadersNombre = { "nombre", "pronombre", "producto", "nombreproducto" };
        private static readonly string[] HeadersCantidad = { "cantidad", "stock", "existencia", "procantidad" };
        private static readonly string[] HeadersPrecio = { "precio", "proprecio", "valor", "costo" };
        private static readonly string[] HeadersProveedorId = { "provid", "proveedorid", "idproveedor" };
        private static readonly string[] HeadersEstado = { "estado", "proestado", "estadoproducto" };

        public static List<ProductoCargaFila> Leer(byte[] contenido, string nombreArchivo)
        {
            if (contenido == null || contenido.Length == 0)
            {
                throw new Exception("El archivo seleccionado esta vacio.");
            }

            string extension = Path.GetExtension(nombreArchivo ?? string.Empty).ToLowerInvariant();
            switch (extension)
            {
                case ".csv":
                    return ConvertirFilas(ParsearCsv(DetectarTexto(contenido)));
                case ".xlsx":
                    return ConvertirFilas(LeerXlsx(contenido));
                case ".xls":
                    return ConvertirFilas(LeerXls(contenido, extension));
                default:
                    throw new Exception("Formato no soportado. Solo se permiten archivos .csv, .xlsx y .xls.");
            }
        }

        private static List<ProductoCargaFila> ConvertirFilas(List<List<string>> filas)
        {
            if (filas == null || filas.Count == 0)
            {
                throw new Exception("El archivo no contiene datos para procesar.");
            }

            int headerIndex = filas.FindIndex(f => f != null && f.Any(c => !string.IsNullOrWhiteSpace(c)));
            if (headerIndex < 0)
            {
                throw new Exception("No se encontro una fila de encabezados valida.");
            }

            var headers = filas[headerIndex].Select(NormalizarHeader).ToList();
            int indexId = BuscarIndiceHeader(headers, HeadersId);
            int indexNombre = BuscarIndiceHeader(headers, HeadersNombre);
            int indexCantidad = BuscarIndiceHeader(headers, HeadersCantidad);
            int indexPrecio = BuscarIndiceHeader(headers, HeadersPrecio);
            int indexProveedorId = BuscarIndiceHeader(headers, HeadersProveedorId);
            int indexEstado = BuscarIndiceHeader(headers, HeadersEstado);

            if (indexNombre < 0)
            {
                throw new Exception("El archivo debe contener una columna de nombre del producto.");
            }

            var resultado = new List<ProductoCargaFila>();
            for (int i = headerIndex + 1; i < filas.Count; i++)
            {
                var fila = filas[i] ?? new List<string>();
                string idTexto = ObtenerValor(fila, indexId).Trim();
                string nombre = ObtenerValor(fila, indexNombre).Trim();
                string cantidadTexto = ObtenerValor(fila, indexCantidad).Trim();
                string precioTexto = ObtenerValor(fila, indexPrecio).Trim();
                string proveedorTexto = ObtenerValor(fila, indexProveedorId).Trim();
                string estadoTexto = ObtenerValor(fila, indexEstado).Trim();

                if (string.IsNullOrWhiteSpace(idTexto) &&
                    string.IsNullOrWhiteSpace(nombre) &&
                    string.IsNullOrWhiteSpace(cantidadTexto) &&
                    string.IsNullOrWhiteSpace(precioTexto) &&
                    string.IsNullOrWhiteSpace(proveedorTexto) &&
                    string.IsNullOrWhiteSpace(estadoTexto))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new Exception($"La fila {i + 1} no tiene nombre de producto.");
                }

                resultado.Add(new ProductoCargaFila
                {
                    NumeroFilaArchivo = i + 1,
                    ProductoId = ParsearEnteroOpcional(idTexto, $"La fila {i + 1} tiene un ID de producto invalido."),
                    NombreProducto = nombre,
                    Cantidad = ParsearEntero(cantidadTexto, 0, $"La fila {i + 1} tiene una cantidad invalida."),
                    Precio = ParsearDecimal(precioTexto, 0m, $"La fila {i + 1} tiene un precio invalido."),
                    ProveedorId = ParsearEnteroOpcional(proveedorTexto, $"La fila {i + 1} tiene un ID de proveedor invalido."),
                    EstadoProducto = ParsearEstado(estadoTexto)
                });
            }

            if (!resultado.Any())
            {
                throw new Exception("No se encontraron filas validas para importar.");
            }

            return resultado;
        }

        private static int BuscarIndiceHeader(List<string> headers, IEnumerable<string> alias)
        {
            return headers.FindIndex(h => alias.Contains(h));
        }

        private static string ObtenerValor(List<string> fila, int indice)
        {
            return indice >= 0 && indice < fila.Count ? (fila[indice] ?? string.Empty) : string.Empty;
        }

        private static int? ParsearEnteroOpcional(string valor, string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            int numero;
            if (!int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out numero) || numero <= 0)
            {
                throw new Exception(mensajeError);
            }

            return numero;
        }

        private static int ParsearEntero(string valor, int porDefecto, string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return porDefecto;
            }

            int numero;
            if (int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out numero))
            {
                return numero;
            }

            decimal decimalNumero;
            if (decimal.TryParse(valor.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimalNumero))
            {
                return (int)Math.Round(decimalNumero, MidpointRounding.AwayFromZero);
            }

            throw new Exception(mensajeError);
        }

        private static decimal ParsearDecimal(string valor, decimal porDefecto, string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return porDefecto;
            }

            decimal numero;
            if (decimal.TryParse(valor.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
            {
                return numero;
            }

            throw new Exception(mensajeError);
        }

        private static char ParsearEstado(string estadoTexto)
        {
            if (string.IsNullOrWhiteSpace(estadoTexto))
            {
                return 'A';
            }

            string valor = NormalizarHeader(estadoTexto);
            if (valor == "a" || valor == "activo") return 'A';
            if (valor == "i" || valor == "inactivo") return 'I';
            throw new Exception($"Estado no valido: '{estadoTexto}'. Usa A/Activo o I/Inactivo.");
        }

        private static string NormalizarHeader(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
            string sinTildes = new string(valor.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return sinTildes.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
        }

        private static string DetectarTexto(byte[] contenido)
        {
            if (contenido.Length >= 3 && contenido[0] == 0xEF && contenido[1] == 0xBB && contenido[2] == 0xBF)
            {
                return Encoding.UTF8.GetString(contenido, 3, contenido.Length - 3);
            }

            try
            {
                return new UTF8Encoding(false, true).GetString(contenido);
            }
            catch
            {
                return Encoding.GetEncoding(1252).GetString(contenido);
            }
        }

        private static List<List<string>> ParsearCsv(string texto)
        {
            var resultado = new List<List<string>>();
            var filaActual = new List<string>();
            var valorActual = new StringBuilder();
            bool enComillas = false;

            for (int i = 0; i < texto.Length; i++)
            {
                char c = texto[i];
                if (enComillas)
                {
                    if (c == '"')
                    {
                        if (i + 1 < texto.Length && texto[i + 1] == '"')
                        {
                            valorActual.Append('"');
                            i++;
                        }
                        else
                        {
                            enComillas = false;
                        }
                    }
                    else
                    {
                        valorActual.Append(c);
                    }
                    continue;
                }

                if (c == '"') enComillas = true;
                else if (c == ',')
                {
                    filaActual.Add(valorActual.ToString());
                    valorActual.Clear();
                }
                else if (c == '\r' || c == '\n')
                {
                    filaActual.Add(valorActual.ToString());
                    valorActual.Clear();
                    if (c == '\r' && i + 1 < texto.Length && texto[i + 1] == '\n') i++;
                    resultado.Add(filaActual);
                    filaActual = new List<string>();
                }
                else valorActual.Append(c);
            }

            filaActual.Add(valorActual.ToString());
            if (filaActual.Any(c => !string.IsNullOrWhiteSpace(c)) || !resultado.Any()) resultado.Add(filaActual);
            return resultado;
        }

        private static List<List<string>> LeerXls(byte[] contenido, string extension)
        {
            string rutaTemporal = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
            File.WriteAllBytes(rutaTemporal, contenido);
            try
            {
                var filas = new List<List<string>>();
                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={rutaTemporal};Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\";";
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    var schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    string sheetName = schema.Rows.Cast<DataRow>()
                        .Select(r => Convert.ToString(r["TABLE_NAME"]))
                        .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
                    using (var adapter = new OleDbDataAdapter($"SELECT * FROM [{sheetName}]", connection))
                    {
                        var table = new DataTable();
                        adapter.Fill(table);
                        filas.Add(table.Columns.Cast<DataColumn>().Select(c => Convert.ToString(c.ColumnName)).ToList());
                        foreach (DataRow row in table.Rows)
                        {
                            filas.Add(table.Columns.Cast<DataColumn>().Select(c => Convert.ToString(row[c] ?? string.Empty)).ToList());
                        }
                    }
                }
                return filas;
            }
            finally
            {
                if (File.Exists(rutaTemporal)) File.Delete(rutaTemporal);
            }
        }

        private static List<List<string>> LeerXlsx(byte[] contenido)
        {
            using (var stream = new MemoryStream(contenido))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                XNamespace pkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

                var workbookEntry = zip.GetEntry("xl/workbook.xml");
                var workbookRelsEntry = zip.GetEntry("xl/_rels/workbook.xml.rels");
                XDocument workbookDoc;
                XDocument workbookRelsDoc;
                using (var workbookStream = workbookEntry.Open())
                using (var workbookReader = new StreamReader(workbookStream))
                {
                    workbookDoc = XDocument.Load(workbookReader);
                }
                using (var relsStream = workbookRelsEntry.Open())
                using (var relsReader = new StreamReader(relsStream))
                {
                    workbookRelsDoc = XDocument.Load(relsReader);
                }

                var firstSheet = workbookDoc.Descendants(ns + "sheet").First();
                string relationId = (string)firstSheet.Attribute(relNs + "id");
                string target = workbookRelsDoc.Descendants(pkgRelNs + "Relationship")
                    .Where(r => string.Equals((string)r.Attribute("Id"), relationId, StringComparison.Ordinal))
                    .Select(r => (string)r.Attribute("Target"))
                    .First();

                var sharedStrings = LeerSharedStrings(zip, ns);
                var filas = new List<List<string>>();
                var sheetEntry = zip.GetEntry("xl/" + target.TrimStart('/').Replace("\\", "/"));
                using (var sheetStream = sheetEntry.Open())
                using (var sheetReader = new StreamReader(sheetStream))
                {
                    var sheetDoc = XDocument.Load(sheetReader);
                    foreach (var row in sheetDoc.Descendants(ns + "row"))
                    {
                        var celdas = new SortedDictionary<int, string>();
                        foreach (var cell in row.Elements(ns + "c"))
                        {
                            string referencia = (string)cell.Attribute("r");
                            int columna = ObtenerIndiceColumna(referencia);
                            celdas[columna] = LeerValorCelda(cell, ns, sharedStrings);
                        }

                        if (!celdas.Any()) continue;
                        int maxColumna = celdas.Keys.Max();
                        var fila = new List<string>();
                        for (int i = 0; i <= maxColumna; i++)
                        {
                            string valor;
                            fila.Add(celdas.TryGetValue(i, out valor) ? valor : string.Empty);
                        }
                        filas.Add(fila);
                    }
                }
                return filas;
            }
        }

        private static List<string> LeerSharedStrings(ZipArchive zip, XNamespace ns)
        {
            var entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return new List<string>();
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                var doc = XDocument.Load(reader);
                return doc.Descendants(ns + "si")
                    .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
                    .ToList();
            }
        }

        private static string LeerValorCelda(XElement cell, XNamespace ns, List<string> sharedStrings)
        {
            string type = (string)cell.Attribute("t");
            if (string.Equals(type, "inlineStr", StringComparison.OrdinalIgnoreCase))
            {
                return string.Concat(cell.Descendants(ns + "t").Select(t => t.Value));
            }

            var valueElement = cell.Element(ns + "v");
            if (valueElement == null) return string.Empty;
            string rawValue = valueElement.Value ?? string.Empty;
            if (string.Equals(type, "s", StringComparison.OrdinalIgnoreCase))
            {
                int index;
                return int.TryParse(rawValue, out index) && index >= 0 && index < sharedStrings.Count ? sharedStrings[index] : string.Empty;
            }
            if (string.Equals(type, "b", StringComparison.OrdinalIgnoreCase))
            {
                return rawValue == "1" ? "TRUE" : "FALSE";
            }
            return rawValue;
        }

        private static int ObtenerIndiceColumna(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia)) return 0;
            int indice = 0;
            foreach (char c in referencia.ToUpperInvariant())
            {
                if (c < 'A' || c > 'Z') break;
                indice = (indice * 26) + (c - 'A' + 1);
            }
            return Math.Max(0, indice - 1);
        }
    }
}
