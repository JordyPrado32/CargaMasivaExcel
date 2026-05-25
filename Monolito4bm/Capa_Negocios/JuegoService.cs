using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios
{
    public class ProgresoJuego
    {
        public int NivelDesbloqueado { get; set; } = 1;
        public int MejorPuntuacion { get; set; } = 0;
        public int MonedasTotales { get; set; } = 0;
    }

    public class RankingEntry
    {
        public string Nombre { get; set; }
        public string Nickname { get; set; }
        public int MejorPuntuacion { get; set; }
        public int Nivel { get; set; }
        public int Monedas { get; set; }
    }

    public class ResultadoPartida
    {
        public int Puntuacion { get; set; }
        public int MonedasRecogidas { get; set; }
        public int NivelCompletado { get; set; }
        public bool NuevoNivelDesbloq { get; set; }
        public bool NuevoRecord { get; set; }
    }

    public class JuegoService
    {
        private ProgresoJuego DB_ObtenerProgresoJuego(int usuId)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                return dc.tbl_juego_progreso
                    .Where(p => p.usu_id == usuId)
                    .Select(p => new ProgresoJuego
                    {
                        NivelDesbloqueado = p.nivel_desbloqueado,
                        MejorPuntuacion = p.mejor_puntuacion,
                        MonedasTotales = p.monedas_totales
                    })
                    .FirstOrDefault();
            }
        }

        private void DB_GuardarProgresoJuego(int usuId, int nivelDesbloqueado, int mejorPuntuacion, int monedasTotales)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var progreso = dc.tbl_juego_progreso.FirstOrDefault(p => p.usu_id == usuId);

                if (progreso == null)
                {
                    progreso = new Capa_Datos.tbl_juego_progreso
                    {
                        usu_id = usuId,
                        nivel_desbloqueado = nivelDesbloqueado,
                        mejor_puntuacion = mejorPuntuacion,
                        monedas_totales = monedasTotales,
                        ultima_partida = DateTime.Now
                    };
                    dc.tbl_juego_progreso.InsertOnSubmit(progreso);
                }
                else
                {
                    progreso.nivel_desbloqueado = Math.Max(progreso.nivel_desbloqueado, nivelDesbloqueado);
                    progreso.mejor_puntuacion = Math.Max(progreso.mejor_puntuacion, mejorPuntuacion);
                    progreso.monedas_totales += monedasTotales;
                    progreso.ultima_partida = DateTime.Now;
                }

                dc.SubmitChanges();
            }
        }

        private List<RankingEntry> DB_ObtenerRanking()
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                return dc.tbl_juego_progreso
                    .OrderByDescending(p => p.mejor_puntuacion)
                    .Take(10)
                    .Select(p => new RankingEntry
                    {
                        Nombre = p.tbl_usuario.usu_nombres,
                        Nickname = p.tbl_usuario.usu_nickname,
                        MejorPuntuacion = p.mejor_puntuacion,
                        Nivel = p.nivel_desbloqueado,
                        Monedas = p.monedas_totales
                    })
                    .ToList();
            }
        }

        public ProgresoJuego ObtenerProgreso(int usuId)
        {
            return DB_ObtenerProgresoJuego(usuId) ?? new ProgresoJuego();
        }

        public ResultadoPartida GuardarPartida(int usuId, int nivelJugado, int puntuacion, int monedasRecogidas)
        {
            var progresoActual = ObtenerProgreso(usuId);

            bool nuevoRecord = puntuacion > progresoActual.MejorPuntuacion;
            bool nivelSuperado = puntuacion >= ObtenerPuntajeMinimo(nivelJugado);
            int nuevoNivel = nivelSuperado ? Math.Min(nivelJugado + 1, 3) : progresoActual.NivelDesbloqueado;
            bool nuevoNivelDesbloq = nivelSuperado && nuevoNivel > progresoActual.NivelDesbloqueado;

            int nivelAGuardar = Math.Max(nuevoNivel, progresoActual.NivelDesbloqueado);
            int puntosAGuardar = Math.Max(puntuacion, progresoActual.MejorPuntuacion);

            DB_GuardarProgresoJuego(usuId, nivelAGuardar, puntosAGuardar, monedasRecogidas);

            return new ResultadoPartida
            {
                Puntuacion = puntuacion,
                MonedasRecogidas = monedasRecogidas,
                NivelCompletado = nivelJugado,
                NuevoNivelDesbloq = nuevoNivelDesbloq,
                NuevoRecord = nuevoRecord
            };
        }

        public List<RankingEntry> ObtenerRanking()
        {
            return DB_ObtenerRanking();
        }

        public int ObtenerPuntajeMinimo(int nivel)
        {
            switch (nivel)
            {
                case 1: return 400;
                case 2: return 1100;
                case 3: return 2400;
                default: return 999999;
            }
        }

        public string ObtenerConfigNivelJson(int nivel)
        {
            switch (nivel)
            {
                case 1:
                    return @"{
                        ""nivel"": 1,
                        ""nombre"": ""Zona Estelar"",
                        ""velocidad"": 4,
                        ""colorFondo"": ""#0a0a2e"",
                        ""colorSuelo"": ""#ff8da1"",
                        ""colorJugador"": ""#ff8da1"",
                        ""puntajeMinimo"": 400,
                        ""obstaculos"": [
                            {""tipo"":""spike"",  ""x"":600},
                            {""tipo"":""spike"",  ""x"":800},
                            {""tipo"":""bloque"", ""x"":1100, ""h"":80},
                            {""tipo"":""vacio"",  ""x"":1400, ""ancho"":120},
                            {""tipo"":""spike"",  ""x"":1700},
                            {""tipo"":""spike"",  ""x"":1750},
                            {""tipo"":""bloque"", ""x"":2000, ""h"":100},
                            {""tipo"":""vacio"",  ""x"":2300, ""ancho"":100},
                            {""tipo"":""spike"",  ""x"":2600},
                            {""tipo"":""bloque"", ""x"":2900, ""h"":80},
                            {""tipo"":""vacio"",  ""x"":3200, ""ancho"":130},
                            {""tipo"":""spike"",  ""x"":3500},
                            {""tipo"":""spike"",  ""x"":3560},
                            {""tipo"":""meta"",   ""x"":3900}
                        ],
                        ""monedas"": [
                            {""x"":500,  ""y"":300},
                            {""x"":700,  ""y"":280},
                            {""x"":1000, ""y"":300},
                            {""x"":1300, ""y"":260},
                            {""x"":1600, ""y"":300},
                            {""x"":1900, ""y"":270},
                            {""x"":2200, ""y"":300},
                            {""x"":2500, ""y"":280},
                            {""x"":2800, ""y"":300},
                            {""x"":3100, ""y"":260},
                            {""x"":3400, ""y"":300},
                            {""x"":3700, ""y"":280}
                        ]
                    }";
                case 2:
                    return @"{
                        ""nivel"": 2,
                        ""nombre"": ""Abismo Neón"",
                        ""velocidad"": 5,
                        ""colorFondo"": ""#0d0d1a"",
                        ""colorSuelo"": ""#00ffcc"",
                        ""colorJugador"": ""#00ffcc"",
                        ""puntajeMinimo"": 1100,
                        ""obstaculos"": [
                            {""tipo"":""spike"",  ""x"":500},
                            {""tipo"":""spike"",  ""x"":560},
                            {""tipo"":""vacio"",  ""x"":800,  ""ancho"":140},
                            {""tipo"":""bloque"", ""x"":1100, ""h"":120},
                            {""tipo"":""spike"",  ""x"":1400},
                            {""tipo"":""spike"",  ""x"":1460},
                            {""tipo"":""spike"",  ""x"":1520},
                            {""tipo"":""vacio"",  ""x"":1800, ""ancho"":150},
                            {""tipo"":""bloque"", ""x"":2100, ""h"":100},
                            {""tipo"":""vacio"",  ""x"":2400, ""ancho"":120},
                            {""tipo"":""spike"",  ""x"":2700},
                            {""tipo"":""spike"",  ""x"":2760},
                            {""tipo"":""bloque"", ""x"":3000, ""h"":130},
                            {""tipo"":""vacio"",  ""x"":3300, ""ancho"":160},
                            {""tipo"":""spike"",  ""x"":3600},
                            {""tipo"":""spike"",  ""x"":3660},
                            {""tipo"":""spike"",  ""x"":3720},
                            {""tipo"":""meta"",   ""x"":4200}
                        ],
                        ""monedas"": [
                            {""x"":450,  ""y"":280},
                            {""x"":700,  ""y"":260},
                            {""x"":1000, ""y"":280},
                            {""x"":1300, ""y"":240},
                            {""x"":1650, ""y"":280},
                            {""x"":1950, ""y"":260},
                            {""x"":2250, ""y"":280},
                            {""x"":2550, ""y"":250},
                            {""x"":2850, ""y"":280},
                            {""x"":3150, ""y"":260},
                            {""x"":3450, ""y"":280},
                            {""x"":3900, ""y"":260},
                            {""x"":4100, ""y"":280}
                        ]
                    }";
                case 3:
                    return @"{
                        ""nivel"": 3,
                        ""nombre"": ""Caos Final"",
                        ""velocidad"": 6,
                        ""colorFondo"": ""#1a0000"",
                        ""colorSuelo"": ""#ff4444"",
                        ""colorJugador"": ""#ffaa00"",
                        ""puntajeMinimo"": 2400,
                        ""obstaculos"": [
                            {""tipo"":""spike"",  ""x"":400},
                            {""tipo"":""spike"",  ""x"":460},
                            {""tipo"":""spike"",  ""x"":520},
                            {""tipo"":""vacio"",  ""x"":700,  ""ancho"":160},
                            {""tipo"":""bloque"", ""x"":1000, ""h"":140},
                            {""tipo"":""spike"",  ""x"":1300},
                            {""tipo"":""spike"",  ""x"":1360},
                            {""tipo"":""spike"",  ""x"":1420},
                            {""tipo"":""vacio"",  ""x"":1600, ""ancho"":170},
                            {""tipo"":""spike"",  ""x"":1900},
                            {""tipo"":""bloque"", ""x"":2200, ""h"":150},
                            {""tipo"":""vacio"",  ""x"":2500, ""ancho"":180},
                            {""tipo"":""spike"",  ""x"":2800},
                            {""tipo"":""spike"",  ""x"":2860},
                            {""tipo"":""spike"",  ""x"":2920},
                            {""tipo"":""vacio"",  ""x"":3100, ""ancho"":160},
                            {""tipo"":""bloque"", ""x"":3400, ""h"":160},
                            {""tipo"":""spike"",  ""x"":3700},
                            {""tipo"":""spike"",  ""x"":3760},
                            {""tipo"":""spike"",  ""x"":3820},
                            {""tipo"":""vacio"",  ""x"":4000, ""ancho"":180},
                            {""tipo"":""spike"",  ""x"":4300},
                            {""tipo"":""spike"",  ""x"":4360},
                            {""tipo"":""meta"",   ""x"":4700}
                        ],
                        ""monedas"": [
                            {""x"":350,  ""y"":270},
                            {""x"":600,  ""y"":250},
                            {""x"":900,  ""y"":270},
                            {""x"":1200, ""y"":240},
                            {""x"":1500, ""y"":270},
                            {""x"":1800, ""y"":250},
                            {""x"":2100, ""y"":270},
                            {""x"":2400, ""y"":240},
                            {""x"":2700, ""y"":270},
                            {""x"":3000, ""y"":250},
                            {""x"":3300, ""y"":270},
                            {""x"":3600, ""y"":240},
                            {""x"":3900, ""y"":270},
                            {""x"":4200, ""y"":250},
                            {""x"":4500, ""y"":270}
                        ]
                    }";
                default:
                    return "{}";
            }
        }
    }
}
