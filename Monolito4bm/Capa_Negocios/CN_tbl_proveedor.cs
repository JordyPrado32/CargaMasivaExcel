using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Capa_Datos;

namespace Capa_Negocios
{
    public class CN_tbl_proveedor
    {
        private static MonolitoDataContext dc = new MonolitoDataContext();

        // ── Listar todos — más reciente primero ─────────────────────
        public static List<tbl_proveedor> Listar()
        {
            return dc.GetTable<tbl_proveedor>()
                     .OrderByDescending(p => p.prov_id)
                     .ToList();
        }

        // ── Solo activos ────────────────────────────────────────────
        public static List<tbl_proveedor> ListarActivos()
        {
            return dc.GetTable<tbl_proveedor>()
                     .Where(p => p.prov_estado == 'A')
                     .OrderBy(p => p.prov_nombre)
                     .ToList();
        }

        // ── Buscar por ID ───────────────────────────────────────────
        public static tbl_proveedor BuscarPorId(int id)
        {
            return dc.GetTable<tbl_proveedor>()
                     .FirstOrDefault(p => p.prov_id == id);
        }

        // ── Verificar nombre duplicado ──────────────────────────────
        public static bool ExisteNombre(string nombre, int idIgnorar = 0)
        {
            return dc.GetTable<tbl_proveedor>()
                     .Any(p => p.prov_nombre == nombre
                               && p.prov_estado == 'A'
                               && p.prov_id != idIgnorar);
        }

        // ── Crear ───────────────────────────────────────────────────
        public static void Guardar(tbl_proveedor proveedor)
        {
            try
            {
                proveedor.prov_estado = 'A';
                dc.GetTable<tbl_proveedor>().InsertOnSubmit(proveedor);
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar proveedor: " + ex.Message);
            }
        }

        // ── Modificar ───────────────────────────────────────────────
        public static void Modificar(tbl_proveedor proveedor)
        {
            try
            {
                var existente = BuscarPorId(proveedor.prov_id)
                    ?? throw new Exception("Proveedor no encontrado.");
                existente.prov_nombre = proveedor.prov_nombre;
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar proveedor: " + ex.Message);
            }
        }

        // ── Activar ─────────────────────────────────────────────────
        public static void Activar(int id)
        {
            try
            {
                var prov = BuscarPorId(id)
                    ?? throw new Exception("Proveedor no encontrado.");
                prov.prov_estado = 'A';
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al activar proveedor: " + ex.Message);
            }
        }

        // ── Desactivar (eliminación lógica) ─────────────────────────
        public static void EliminarLogico(int id)
        {
            try
            {
                var prov = BuscarPorId(id)
                    ?? throw new Exception("Proveedor no encontrado.");
                prov.prov_estado = 'I';
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desactivar proveedor: " + ex.Message);
            }
        }
        public static void EliminarFisico(int id)
        {
            // Usamos el DataContext que ya tienes declarado arriba en esta misma clase
            // para verificar la relación desde la capa de negocios.
            if (dc.GetTable<tbl_producto>().Any(p => p.prov_id == id))
            {
                throw new Exception("No se puede borrar de la base porque tiene productos, puede desactivar este proveedor.");
            }

            try
            {
                var prov = BuscarPorId(id) ?? throw new Exception("Proveedor no encontrado.");
                dc.GetTable<tbl_proveedor>().DeleteOnSubmit(prov);
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar proveedor: " + ex.Message);
            }
        }
    }
}