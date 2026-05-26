<%@ WebHandler Language="C#" Class="ImagenUsuarioFallback" %>

using System;
using System.IO;
using System.Web;
using Capa_Negocios;

public class ImagenUsuarioFallback : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        int fotoId;
        if (!int.TryParse(context.Request.QueryString["id"], out fotoId))
        {
            Log(context, "Parametro id invalido");
            context.Response.StatusCode = 400;
            return;
        }

        Log(context, "Solicitando foto id=" + fotoId);

        var foto = CN_tbl_foto.ObtenerParaResolver(fotoId);
        if (foto == null)
        {
            Log(context, "No existe registro para foto id=" + fotoId);
            context.Response.StatusCode = 404;
            return;
        }

        byte[] contenido = null;
        if (foto.foto != null && foto.foto.Length > 0)
        {
            contenido = foto.foto.ToArray();
            Log(context, "Sirviendo desde varbinary. bytes=" + contenido.Length + " ruta=" + (foto.foto_ruta ?? "(null)"));
        }
        else
        {
            string rutaFisica = ResolverRutaFisica(context, foto.foto_ruta);
            if (!string.IsNullOrWhiteSpace(rutaFisica) && File.Exists(rutaFisica))
            {
                contenido = File.ReadAllBytes(rutaFisica);
                Log(context, "Varbinary vacio. Sirviendo desde foto_ruta=" + foto.foto_ruta + " fisica=" + rutaFisica + " bytes=" + contenido.Length);
            }
            else
            {
                Log(context, "Sin varbinary y sin archivo fisico. ruta=" + (foto.foto_ruta ?? "(null)") + " fisica=" + (rutaFisica ?? "(null)"));
                context.Response.StatusCode = 404;
                return;
            }
        }

        context.Response.Clear();
        context.Response.Buffer = true;
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.ContentType = DetectarMime(contenido, foto.nombre_archivo, foto.content_type, foto.foto_ruta);
        context.Response.OutputStream.Write(contenido, 0, contenido.Length);
    }

    public bool IsReusable
    {
        get { return false; }
    }

    private static string DetectarMime(byte[] contenido, string nombreArchivo, string contentType, string ruta)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType;
        }

        if (contenido != null && contenido.Length >= 4)
        {
            if (contenido[0] == 0x89 && contenido[1] == 0x50 && contenido[2] == 0x4E && contenido[3] == 0x47)
            {
                return "image/png";
            }

            if (contenido[0] == 0xFF && contenido[1] == 0xD8)
            {
                return "image/jpeg";
            }

            if (contenido[0] == 0x47 && contenido[1] == 0x49 && contenido[2] == 0x46)
            {
                return "image/gif";
            }
        }

        string extension = Path.GetExtension(ruta ?? nombreArchivo ?? string.Empty).ToLowerInvariant();
        switch (extension)
        {
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".webp":
                return "image/webp";
            default:
                return "image/jpeg";
        }
    }

    private static string ResolverRutaFisica(HttpContext context, string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return null;
        }

        string rutaNormalizada = ruta.Replace("\\", "/").Trim();
        if (!rutaNormalizada.StartsWith("~/", StringComparison.Ordinal))
        {
            rutaNormalizada = "~/" + rutaNormalizada.TrimStart('/');
        }

        return context.Server.MapPath(rutaNormalizada);
    }

    private static void Log(HttpContext context, string mensaje)
    {
        try
        {
            string carpeta = context.Server.MapPath("~/Uploads");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string archivo = Path.Combine(carpeta, "avatar-debug.log");
            string linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] HANDLER {mensaje}{Environment.NewLine}";
            File.AppendAllText(archivo, linea);
        }
        catch
        {
        }
    }
}
