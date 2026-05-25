using System;
using System.Collections.Generic;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    public class DTO_ProveedorStats
    {
        public string NombreProveedor { get; set; }
        public int TotalProductos { get; set; }
        public decimal ValorTotalInventario { get; set; }
    }

    public class DTO_TopRanking
    {
        public string NombreProducto { get; set; }
        public decimal Valor { get; set; }
    }

    public class DTO_Distribucion
    {
        public string Categoria { get; set; }
        public int Cantidad { get; set; }
    }

    public class CN_Estadisticas
    {
        public static List<DTO_ProveedorStats> ObtenerEstadisticasPorProveedor()
        {
            using (var dc = new MonolitoDataContext())
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
        }

        public static List<DTO_TopRanking> ObtenerTop5MasCaros()
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_producto>()
                         .Where(p => p.pro_estado == 'A')
                         .OrderByDescending(p => p.pro_precio)
                         .Take(5)
                         .Select(p => new DTO_TopRanking
                         {
                             NombreProducto = p.pro_nombre,
                             Valor = p.pro_precio ?? 0m
                         }).ToList();
            }
        }

        public static List<DTO_TopRanking> ObtenerTop5MayorStock()
        {
            using (var dc = new MonolitoDataContext())
            {
                return dc.GetTable<tbl_producto>()
                         .Where(p => p.pro_estado == 'A')
                         .OrderByDescending(p => p.pro_cantidad)
                         .Take(5)
                         .Select(p => new DTO_TopRanking
                         {
                             NombreProducto = p.pro_nombre,
                             Valor = p.pro_cantidad ?? 0
                         }).ToList();
            }
        }

        public static List<DTO_Distribucion> ObtenerDistribucionEstados()
        {
            using (var dc = new MonolitoDataContext())
            {
                return (from p in dc.GetTable<tbl_producto>()
                        group p by p.pro_estado into grupo
                        select new DTO_Distribucion
                        {
                            Categoria = grupo.Key == 'A' ? "Activos" : "Inactivos",
                            Cantidad = grupo.Count()
                        }).ToList();
            }
        }

        public static List<DTO_Distribucion> ObtenerSaludInventario()
        {
            using (var dc = new MonolitoDataContext())
            {
                var productos = dc.GetTable<tbl_producto>()
                    .Where(p => p.pro_estado == 'A')
                    .ToList();

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
                        Categoria = "Stock Optimo (Mas de 50)",
                        Cantidad = productos.Count(p => p.pro_cantidad > 50)
                    }
                };
            }
        }
    }
}
