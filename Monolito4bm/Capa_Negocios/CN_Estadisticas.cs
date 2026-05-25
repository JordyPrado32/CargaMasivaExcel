using System;
using System.Collections.Generic;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    // ====================================================================
    // 1. CLASES AUXILIARES (DTOs) - Las columnas que verá el ReportViewer
    // ====================================================================

    // DTO para gráficos agrupados por Proveedor (Sirve para 2 gráficos)
    public class DTO_ProveedorStats
    {
        public string NombreProveedor { get; set; }
        public int TotalProductos { get; set; }
        public decimal ValorTotalInventario { get; set; }
    }

    // DTO para gráficos de tipo "Top 5" (Sirve para Ranking de precios o stock)
    public class DTO_TopRanking
    {
        public string NombreProducto { get; set; }
        public decimal Valor { get; set; } // Puede representar Dinero o Cantidad según el método
    }

    // DTO para gráficos de distribución (Sirve para Estados y Niveles de Stock)
    public class DTO_Distribucion
    {
        public string Categoria { get; set; }
        public int Cantidad { get; set; }
    }

    public class CN_Estadisticas
    {
        private static MonolitoDataContext dc = new MonolitoDataContext();

        // 📊 GRÁFICO 1 y 2: Cantidad de Productos y Valor de Inventario por Proveedor
        // Sugerencia visual: Gráfico circular (Pastel) para los totales, y de Barras para el dinero.
        public static List<DTO_ProveedorStats> ObtenerEstadisticasPorProveedor()
        {
            return (from p in dc.GetTable<tbl_producto>()
                    where p.pro_estado == 'A'
                    group p by p.tbl_proveedor.prov_nombre into grupo
                    select new DTO_ProveedorStats
                    {
                        NombreProveedor = grupo.Key,
                        TotalProductos = grupo.Count(),
                        ValorTotalInventario = grupo.Sum(x => (x.pro_cantidad ?? 0) * (x.pro_precio ?? 0m))
                    }).ToList();
        }
        // Sugerencia visual: Gráfico de Barras Horizontales.
        // 📊 GRÁFICO 3: Top 5 Productos más caros
        public static List<DTO_TopRanking> ObtenerTop5MasCaros()
        {
            return dc.GetTable<tbl_producto>()
                     .Where(p => p.pro_estado == 'A')
                     .OrderByDescending(p => p.pro_precio)
                     .Take(5)
                     .Select(p => new DTO_TopRanking
                     {
                         NombreProducto = p.pro_nombre,
                         Valor = p.pro_precio ?? 0m  // <-- Agregamos ?? 0m aquí
                     }).ToList();
        }

        // 📊 GRÁFICO 4: Top 5 Productos con mayor cantidad en Stock
        public static List<DTO_TopRanking> ObtenerTop5MayorStock()
        {
            return dc.GetTable<tbl_producto>()
                     .Where(p => p.pro_estado == 'A')
                     .OrderByDescending(p => p.pro_cantidad)
                     .Take(5)
                     .Select(p => new DTO_TopRanking
                     {
                         NombreProducto = p.pro_nombre,
                         Valor = p.pro_cantidad ?? 0  // <-- Agregamos ?? 0 aquí
                     }).ToList();
        }

        // 📊 GRÁFICO 5: Distribución de productos por Estado (Activos vs Inactivos)
        // Sugerencia visual: Gráfico de Anillo (Doughnut).
        public static List<DTO_Distribucion> ObtenerDistribucionEstados()
        {
            return (from p in dc.GetTable<tbl_producto>()
                    group p by p.pro_estado into grupo
                    select new DTO_Distribucion
                    {
                        // Convertimos la 'A' y la 'I' en palabras completas para que el gráfico se entienda
                        Categoria = grupo.Key == 'A' ? "Activos" : "Inactivos",
                        Cantidad = grupo.Count()
                    }).ToList();
        }

        // 📊 GRÁFICO 6: Análisis de Salud del Inventario (Niveles de Stock)
        // Sugerencia visual: Gráfico de Embudo (Funnel) o Pirámide.
        public static List<DTO_Distribucion> ObtenerSaludInventario()
        {
            // Traemos los activos a memoria primero para hacer el conteo personalizado sin que LINQ to SQL explote
            var productos = dc.GetTable<tbl_producto>().Where(p => p.pro_estado == 'A').ToList();

            return new List<DTO_Distribucion>
            {
                new DTO_Distribucion
                {
                    Categoria = "Stock Bajo (Menos de 10)",
                    Cantidad = productos.Count(p => p.pro_cantidad < 10)
                },
                new DTO_Distribucion
                {
                    Categoria = "Stock Medio (10 a 50)",
                    Cantidad = productos.Count(p => p.pro_cantidad >= 10 && p.pro_cantidad <= 50)
                },
                new DTO_Distribucion
                {
                    Categoria = "Stock Óptimo (Más de 50)",
                    Cantidad = productos.Count(p => p.pro_cantidad > 50)
                }
            };
        }
    }
}