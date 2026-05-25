using Capa_Negocios;
using System;
using System.Web.Script.Serialization;
using System.Web.UI; // Importante para el ScriptManager

namespace Monolito4bm
{
    public partial class PaginaRol2 : System.Web.UI.Page
    {
        private readonly JuegoService _juegoSvc = new JuegoService();
        // Agregamos el servicio de usuario para no usar la Capa de Datos
        private readonly UsuarioService _usuSvc = new UsuarioService();

        // ── Propiedades 100% Públicas para evitar errores de contexto en el ASPX ──
        public int UsuId { get; set; }
        public string NombreUsuario { get; set; }
        public string ProgresoJson { get; set; }
        public string ConfigNivel1Json { get; set; }
        public string ConfigNivel2Json { get; set; }
        public string ConfigNivel3Json { get; set; }
        public string RankingJson { get; set; }
        // ─────────────────────────────────────────────────────────────────────────

        protected void Page_Load(object sender, EventArgs e)
        {
            // Seguridad: solo accede si está autenticado
            if (Session["autenticado"] == null || !(bool)Session["autenticado"])
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            UsuId = ObtenerUsuIdDeSesion();
            NombreUsuario = Session["nombre_usuario"]?.ToString() ?? "Jugador";

            // Se carga la info del juego siempre, evitando llamadas innecesarias después
            if (!IsPostBack)
            {
                CargarDatos();
            }
        }

        private void CargarDatos()
        {
            var progreso = _juegoSvc.ObtenerProgreso(UsuId);

            // Serializar progreso a JSON para el JS
            ProgresoJson = new JavaScriptSerializer().Serialize(new
            {
                nivelDesbloqueado = progreso.NivelDesbloqueado,
                mejorPuntuacion = progreso.MejorPuntuacion,
                monedasTotales = progreso.MonedasTotales
            });

            // Pasar configuración de cada nivel al cliente
            ConfigNivel1Json = _juegoSvc.ObtenerConfigNivelJson(1);
            ConfigNivel2Json = _juegoSvc.ObtenerConfigNivelJson(2);
            ConfigNivel3Json = _juegoSvc.ObtenerConfigNivelJson(3);

            // Traer el ranking directamente para evitar el bucle de postback
            var ranking = _juegoSvc.ObtenerRanking();
            RankingJson = new JavaScriptSerializer().Serialize(ranking);
        }

        // ── Guardar partida en segundo plano gracias al UpdatePanel ─────────
        protected void btnGuardarPartida_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnNivel.Value, out int nivel)) nivel = 1;
            if (!int.TryParse(hdnPuntuacion.Value, out int puntos)) puntos = 0;
            if (!int.TryParse(hdnMonedas.Value, out int monedas)) monedas = 0;

            var resultado = _juegoSvc.GuardarPartida(UsuId, nivel, puntos, monedas);

            // Recargar datos actualizados de la base de datos
            var progreso = _juegoSvc.ObtenerProgreso(UsuId);
            var ranking = _juegoSvc.ObtenerRanking();

            string msg = resultado.NuevoRecord ? "Nuevo record personal!" :
                         resultado.NuevoNivelDesbloq ? $"Nivel {nivel + 1} desbloqueado!" :
                                                       "Partida guardada.";

            // Script que se inyectará de forma silenciosa para actualizar el frontend
            string script = $@"
                PROGRESO.nivelDesbloqueado = {progreso.NivelDesbloqueado};
                PROGRESO.mejorPuntuacion = {progreso.MejorPuntuacion};
                PROGRESO.monedasTotales = {progreso.MonedasTotales};
                actualizarUI();
                desbloquearNiveles(PROGRESO.nivelDesbloqueado);

                RANKING_DATA = {new JavaScriptSerializer().Serialize(ranking)};
                mostrarRanking(RANKING_DATA);

                if ({resultado.NuevoRecord.ToString().ToLower()} || {resultado.NuevoNivelDesbloq.ToString().ToLower()}) {{
                    Swal.fire({{
                        icon: '{(resultado.NuevoNivelDesbloq ? "success" : "info")}',
                        title: '{msg}',
                        text: 'Puntuacion: {resultado.Puntuacion}',
                        confirmButtonColor: '#ff8da1',
                        background: '#ffffff',
                        color: '#5d3f6a'
                    }});
                }}
            ";

            // Usamos ScriptManager para que no recargue la página y rompa el juego
            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateGame", script, true);
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear(); // Limpia todas las variables de la sesión
            Response.Redirect("~/Default.aspx"); // Redirige al login
        }

        // ── Helpers ───────────────────────────────────────────────
        private int ObtenerUsuIdDeSesion()
        {
            if (Session["usu_id"] != null)
                return Convert.ToInt32(Session["usu_id"]);

            if (Session["correo_qr"] != null)
            {
                // AQUÍ OCURRE LA MAGIA: Llamamos a Capa_Negocios, no a Capa_Datos
                int id = _usuSvc.ObtenerIdPorNick(Session["correo_qr"].ToString());
                if (id > 0)
                {
                    Session["usu_id"] = id;
                    return id;
                }
            }
            return 0;
        }
    }
}