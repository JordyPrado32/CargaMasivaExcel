<%@ WebHandler Language="C#" Class="ImagenProductoFallback" %>

using System;
using System.IO;
using System.Web;
using Capa_Negocios;

public class ImagenProductoFallback : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        int fotoId;
        if (!int.TryParse(context.Request.QueryString["id"], out fotoId) || fotoId <= 0)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var foto = CN_tbl_pro_fotos.ObtenerParaResolver(fotoId);
        if (foto == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        string rutaVirtual = NormalizarRutaVirtual(foto.foto_ruta);
        string rutaFisica = string.IsNullOrWhiteSpace(rutaVirtual) ? null : context.Server.MapPath(rutaVirtual);

        if (!string.IsNullOrWhiteSpace(rutaFisica) && File.Exists(rutaFisica))
        {
            byte[] archivo = File.ReadAllBytes(rutaFisica);
            context.Response.Clear();
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetMaxAge(TimeSpan.FromHours(1));
            context.Response.ContentType = DetectarMime(archivo, rutaVirtual);
            context.Response.BinaryWrite(archivo);
            return;
        }

        if (foto.foto_bit == null || foto.foto_bit.Length == 0)
        {
            context.Response.StatusCode = 404;
            return;
        }

        byte[] contenido = foto.foto_bit.ToArray();
        context.Response.Clear();
        context.Response.Cache.SetCacheability(HttpCacheability.Public);
        context.Response.Cache.SetMaxAge(TimeSpan.FromHours(1));
        context.Response.ContentType = DetectarMime(contenido, rutaVirtual);
        context.Response.BinaryWrite(contenido);
    }

    public bool IsReusable { get { return true; } }

    private static string NormalizarRutaVirtual(string ruta)
    {
        string limpia = (ruta ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(limpia))
        {
            return null;
        }

        limpia = limpia.TrimStart('~').TrimStart('/').Replace("\\", "/");
        return "~/" + limpia;
    }

    private static string DetectarMime(byte[] bytes, string ruta)
    {
        if (bytes != null && bytes.Length >= 4)
        {
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return "image/png";
            }

            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                return "image/jpeg";
            }

            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
            {
                return "image/gif";
            }
        }

        string extension = VirtualPathUtility.GetExtension(ruta ?? string.Empty);
        if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
        {
            return "image/png";
        }

        if (string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase))
        {
            return "image/gif";
        }

        return "image/jpeg";
    }
}
