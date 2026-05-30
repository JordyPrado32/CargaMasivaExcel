using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Capa_Negocios
{
    public enum ResultadoLogin { Exitoso, ExitosoClaveTemporalActiva, CredencialesInvalidas, UsuarioBloqueado, ErrorInterno }
    public enum ResultadoRecuperacion { Exitoso, UsuarioNoEncontrado, ErrorInterno }

    public class UsuarioService
    {
        private static string SmtpHost => AppSettingsSecrets.GetRequiredString("Smtp", "Host");
        private static int SmtpPuerto => AppSettingsSecrets.GetRequiredInt("Smtp", "Port");
        private static string SmtpUsuario => AppSettingsSecrets.GetRequiredString("Smtp", "Username");
        private static string SmtpPassword => AppSettingsSecrets.GetRequiredString("Smtp", "Password");
        private const string RemitenteNombre = "Carga Excel";

        private static string TwilioAccountSid => AppSettingsSecrets.GetRequiredString("Twilio", "AccountSid");
        private static string TwilioAuthToken => AppSettingsSecrets.GetRequiredString("Twilio", "AuthToken");
        private static string TwilioNumero => AppSettingsSecrets.GetRequiredString("Twilio", "FromNumber");

        public ResultadoLogin IniciarSesion(string nick, string passwordIngresada, out int rolId, out string correo, out string mensajeErr)
        {
            rolId = 0;
            correo = string.Empty;
            mensajeErr = string.Empty;

            try
            {
                DB_ResetearIntentosCaducados();
                var dt = DB_ObtenerUsuarioPorNick(nick);
                if (dt.Rows.Count == 0)
                {
                    mensajeErr = "Usuario no encontrado.";
                    return ResultadoLogin.CredencialesInvalidas;
                }

                var row = dt.Rows[0];
                int usuId = Convert.ToInt32(row["usu_id"]);
                if (Convert.ToInt32(row["intentos_fallidos"]) >= 3) return ResultadoLogin.UsuarioBloqueado;

                if (!BCrypt.Net.BCrypt.Verify(passwordIngresada, row["contraseña"].ToString()))
                {
                    DB_IncrementarIntentosFallidos(usuId);
                    return ResultadoLogin.CredencialesInvalidas;
                }

                DB_ResetearIntentos(usuId);
                rolId = Convert.ToInt32(row["rol_id"]);
                correo = row["correo_electronico"].ToString();
                string nombre = row["usu_nombres"].ToString();
                string celular = row.Table.Columns.Contains("numero_celular") ? row["numero_celular"].ToString() : string.Empty;

                if (Convert.ToBoolean(row["clave_temporal"])) return ResultadoLogin.ExitosoClaveTemporalActiva;

                string otp = GenerarOtp();
                DB_GuardarOtp(usuId, BCrypt.Net.BCrypt.HashPassword(otp), DateTime.Now);

                string nombreCorto = ObtenerPrimerNombreApellido(nombre);
                try { EnviarAccesoMfaPorCorreo(correo, nombreCorto, otp); } catch { }
                if (!string.IsNullOrEmpty(celular)) { try { EnviarOtpPorWhatsApp(celular, nombreCorto, otp); } catch { } }

                return ResultadoLogin.Exitoso;
            }
            catch (Exception ex)
            {
                mensajeErr = ex.Message;
                return ResultadoLogin.ErrorInterno;
            }
        }

        public int ValidarOtp(string otpIngresado)
        {
            var dt = DB_ObtenerOtpVigente();
            foreach (DataRow row in dt.Rows)
            {
                if (BCrypt.Net.BCrypt.Verify(otpIngresado, row["codigo_otp"].ToString()))
                {
                    DB_InvalidarOtpPorUsuario(Convert.ToInt32(row["usu_id"]));
                    return Convert.ToInt32(row["rol_id"]);
                }
            }
            return -1;
        }

        public ResultadoRecuperacion RecuperarClave(string nickOCorreo, out string mensajeErr)
        {
            mensajeErr = string.Empty;
            try
            {
                var dt = DB_ObtenerUsuarioPorNickOCorreo(nickOCorreo);
                if (dt.Rows.Count == 0)
                {
                    mensajeErr = "No encontrado.";
                    return ResultadoRecuperacion.UsuarioNoEncontrado;
                }

                var row = dt.Rows[0];
                int usuId = Convert.ToInt32(row["usu_id"]);
                string nombre = row["usu_nombres"].ToString();
                string celular = row["numero_celular"].ToString();
                string correo = row["correo_electronico"].ToString();

                string claveTemporal = GenerarClaveTemporal();
                DB_GuardarClaveTemporal(usuId, BCrypt.Net.BCrypt.HashPassword(claveTemporal));
                string nombreCorto = ObtenerPrimerNombreApellido(nombre);

                bool enviado = false;
                if (!string.IsNullOrWhiteSpace(celular)) { try { EnviarSmsTwilio(celular, nombreCorto, claveTemporal); enviado = true; } catch { } }
                if (!enviado && !string.IsNullOrWhiteSpace(correo)) { try { EnviarClaveTemporalPorCorreo(correo, nombreCorto, claveTemporal); } catch { } }

                mensajeErr = "Clave temporal generada.";
                return ResultadoRecuperacion.Exitoso;
            }
            catch (Exception ex)
            {
                mensajeErr = ex.Message;
                return ResultadoRecuperacion.ErrorInterno;
            }
        }

        public void CambiarContrasena(int usuId, string nuevaClave)
        {
            DB_ActualizarContrasena(usuId, BCrypt.Net.BCrypt.HashPassword(nuevaClave));
        }

        public DataTable ObtenerEstadoCuentas() => DB_ObtenerEstadoCuentas();

        public void ResetearIntentosUsuario(int usuId) => DB_ResetearIntentosAdmin(usuId);

        public void DesbloquearCuentaConClaveTemporal(int usuId, out string correoDestino, out string claveTemporal)
        {
            var dt = DB_ObtenerUsuarioPorId(usuId);
            if (dt.Rows.Count == 0) throw new Exception("Cuenta no encontrada.");

            string correo = dt.Rows[0]["correo_electronico"].ToString();
            claveTemporal = GenerarClaveTemporal();
            DB_GuardarClaveTemporal(usuId, BCrypt.Net.BCrypt.HashPassword(claveTemporal));
            DB_ResetearIntentosAdmin(usuId);

            try { EnviarClaveTemporalPorCorreo(correo, ObtenerPrimerNombreApellido(dt.Rows[0]["usu_nombres"].ToString()), claveTemporal); } catch { }
            correoDestino = correo;
        }

        private void EnviarAccesoMfaPorCorreo(string destinatario, string nombre, string otp)
        {
            string urlQr = "https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=" + Uri.EscapeDataString("OTP:" + otp);
            string cuerpo = $"<html><body><h2>Verificación</h2><img src='{urlQr}'/><br><h1>{otp}</h1></body></html>";
            using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto) { Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword), EnableSsl = true })
            {
                smtp.Send(new MailMessage(SmtpUsuario, destinatario, "Carga Excel", cuerpo) { IsBodyHtml = true });
            }
        }

        private void EnviarClaveTemporalPorCorreo(string destinatario, string nombre, string claveTemporal)
        {
            string cuerpo = $"<html><body><h2>Recuperación</h2><h1>{claveTemporal}</h1></body></html>";
            using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto) { Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword), EnableSsl = true })
            {
                smtp.Send(new MailMessage(SmtpUsuario, destinatario, "Clave Temporal", cuerpo) { IsBodyHtml = true });
            }
        }

        private void EnviarOtpPorWhatsApp(string celular, string nombre, string otp)
        {
            string num = celular.StartsWith("0") ? "+593" + celular.Substring(1) : "+" + celular;
            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);
            MessageResource.Create(to: new Twilio.Types.PhoneNumber("whatsapp:" + num), from: new Twilio.Types.PhoneNumber("whatsapp:" + TwilioNumero), body: $"Tu código es: *{otp}*");
        }

        private void EnviarSmsTwilio(string celular, string nombre, string clave)
        {
            string num = celular.StartsWith("0") ? "+593" + celular.Substring(1) : "+" + celular;
            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);
            MessageResource.Create(to: new Twilio.Types.PhoneNumber("whatsapp:" + num), from: new Twilio.Types.PhoneNumber("whatsapp:" + TwilioNumero), body: $"Clave temporal: {clave}");
        }

        private string GenerarOtp()
        {
            byte[] data = new byte[8];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(data);
            return new string(data.Select(b => "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[b % 32]).ToArray());
        }

        private string GenerarClaveTemporal()
        {
            byte[] data = new byte[10];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(data);
            return new string(data.Select(b => "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789"[b % 54]).ToArray());
        }

        private string ObtenerPrimerNombreApellido(string completo)
        {
            var p = completo.Trim().Split(' ');
            return p.Length >= 3 ? $"{p[0]} {p[2]}" : p[0];
        }

        public int ObtenerIdPorNick(string nick)
        {
            var dt = DB_ObtenerUsuarioPorNick(nick);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["usu_id"]) : 0;
        }

        private DataTable CrearTablaUsuarioLogin()
        {
            var dt = new DataTable();
            dt.Columns.Add("usu_id", typeof(int));
            dt.Columns.Add("contraseña", typeof(string));
            dt.Columns.Add("rol_id", typeof(int));
            dt.Columns.Add("intentos_fallidos", typeof(int));
            dt.Columns.Add("correo_electronico", typeof(string));
            dt.Columns.Add("usu_nombres", typeof(string));
            dt.Columns.Add("clave_temporal", typeof(bool));
            dt.Columns.Add("numero_celular", typeof(string));
            return dt;
        }

        private DataTable CrearTablaUsuarioRecuperacion()
        {
            var dt = new DataTable();
            dt.Columns.Add("usu_id", typeof(int));
            dt.Columns.Add("usu_nombres", typeof(string));
            dt.Columns.Add("numero_celular", typeof(string));
            dt.Columns.Add("correo_electronico", typeof(string));
            return dt;
        }

        private DataTable CrearTablaEstadoCuentas()
        {
            var dt = new DataTable();
            dt.Columns.Add("usu_id", typeof(int));
            dt.Columns.Add("usu_nombres", typeof(string));
            dt.Columns.Add("usu_nickname", typeof(string));
            dt.Columns.Add("correo_electronico", typeof(string));
            dt.Columns.Add("intentos_fallidos", typeof(int));
            dt.Columns.Add("estado_cuenta", typeof(string));
            dt.Columns.Add("ultimo_intento", typeof(DateTime));
            dt.Columns.Add("rol_id", typeof(int));
            return dt;
        }

        private DataTable CrearTablaUsuarioAdmin()
        {
            var dt = new DataTable();
            dt.Columns.Add("usu_id", typeof(int));
            dt.Columns.Add("usu_nombres", typeof(string));
            dt.Columns.Add("usu_nickname", typeof(string));
            dt.Columns.Add("correo_electronico", typeof(string));
            dt.Columns.Add("numero_celular", typeof(string));
            dt.Columns.Add("intentos_fallidos", typeof(int));
            dt.Columns.Add("rol_id", typeof(int));
            dt.Columns.Add("clave_temporal", typeof(bool));
            return dt;
        }

        private DataTable CrearTablaOtp()
        {
            var dt = new DataTable();
            dt.Columns.Add("usu_id", typeof(int));
            dt.Columns.Add("rol_id", typeof(int));
            dt.Columns.Add("codigo_otp", typeof(string));
            return dt;
        }

        private DataTable DB_ObtenerUsuarioPorNick(string nick)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuarios = dc.tbl_usuario
                    .Where(u => u.usu_nickname == nick || u.correo_electronico == nick)
                    .Select(u => new
                    {
                        u.usu_id,
                        u.contraseña,
                        u.rol_id,
                        u.intentos_fallidos,
                        u.correo_electronico,
                        u.usu_nombres,
                        u.clave_temporal,
                        u.numero_celular
                    })
                    .ToList();

                var dt = CrearTablaUsuarioLogin();
                foreach (var u in usuarios)
                {
                    dt.Rows.Add(u.usu_id, u.contraseña, u.rol_id, u.intentos_fallidos, u.correo_electronico, u.usu_nombres, u.clave_temporal, u.numero_celular ?? string.Empty);
                }
                return dt;
            }
        }

        private DataTable DB_ObtenerUsuarioPorNickOCorreo(string valor)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuarios = dc.tbl_usuario
                    .Where(u => u.usu_nickname == valor || u.correo_electronico == valor)
                    .Select(u => new
                    {
                        u.usu_id,
                        u.usu_nombres,
                        u.numero_celular,
                        u.correo_electronico
                    })
                    .ToList();

                var dt = CrearTablaUsuarioRecuperacion();
                foreach (var u in usuarios)
                {
                    dt.Rows.Add(u.usu_id, u.usu_nombres, u.numero_celular ?? string.Empty, u.correo_electronico);
                }
                return dt;
            }
        }

        private DataTable DB_ObtenerEstadoCuentas()
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuarios = dc.tbl_usuario
                    .OrderBy(u => u.rol_id)
                    .ThenBy(u => u.usu_nombres)
                    .Select(u => new
                    {
                        u.usu_id,
                        u.usu_nombres,
                        u.usu_nickname,
                        u.correo_electronico,
                        u.intentos_fallidos,
                        estado_cuenta = u.intentos_fallidos >= 3 ? "Bloqueada" : "Activa",
                        ultimo_intento = u.fecha_otp_generado,
                        u.rol_id
                    })
                    .ToList();

                var dt = CrearTablaEstadoCuentas();
                foreach (var u in usuarios)
                {
                    var row = dt.NewRow();
                    row["usu_id"] = u.usu_id;
                    row["usu_nombres"] = u.usu_nombres;
                    row["usu_nickname"] = u.usu_nickname ?? string.Empty;
                    row["correo_electronico"] = u.correo_electronico;
                    row["intentos_fallidos"] = u.intentos_fallidos;
                    row["estado_cuenta"] = u.estado_cuenta;
                    row["ultimo_intento"] = u.ultimo_intento.HasValue ? (object)u.ultimo_intento.Value : DBNull.Value;
                    row["rol_id"] = u.rol_id;
                    dt.Rows.Add(row);
                }
                return dt;
            }
        }

        private DataTable DB_ObtenerUsuarioPorId(int id)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuarios = dc.tbl_usuario
                    .Where(u => u.usu_id == id)
                    .Select(u => new
                    {
                        u.usu_id,
                        u.usu_nombres,
                        u.usu_nickname,
                        u.correo_electronico,
                        u.numero_celular,
                        u.intentos_fallidos,
                        u.rol_id,
                        u.clave_temporal
                    })
                    .ToList();

                var dt = CrearTablaUsuarioAdmin();
                foreach (var u in usuarios)
                {
                    dt.Rows.Add(u.usu_id, u.usu_nombres, u.usu_nickname ?? string.Empty, u.correo_electronico, u.numero_celular ?? string.Empty, u.intentos_fallidos, u.rol_id, u.clave_temporal);
                }
                return dt;
            }
        }

        private DataTable DB_ObtenerOtpVigente()
        {
            DateTime limite = DateTime.Now.AddMinutes(-5);
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var codigos = dc.tbl_usuario
                    .Where(u => u.codigo_otp != null && u.fecha_otp_generado.HasValue && u.fecha_otp_generado.Value >= limite)
                    .Select(u => new
                    {
                        u.usu_id,
                        u.rol_id,
                        u.codigo_otp
                    })
                    .ToList();

                var dt = CrearTablaOtp();
                foreach (var c in codigos)
                {
                    dt.Rows.Add(c.usu_id, c.rol_id, c.codigo_otp);
                }
                return dt;
            }
        }

        private void DB_IncrementarIntentosFallidos(int id)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.intentos_fallidos += 1;
                dc.SubmitChanges();
            }
        }

        private void DB_ResetearIntentos(int id)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.intentos_fallidos = 0;
                dc.SubmitChanges();
            }
        }

        private void DB_ResetearIntentosAdmin(int id)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.intentos_fallidos = 0;
                usuario.codigo_otp = null;
                usuario.fecha_otp_generado = null;
                dc.SubmitChanges();
            }
        }

        private void DB_ResetearIntentosCaducados()
        {
            DateTime limite = DateTime.Now.AddHours(-24);
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuarios = dc.tbl_usuario
                    .Where(u => u.intentos_fallidos > 0 &&
                                u.intentos_fallidos <= 2 &&
                                u.fecha_otp_generado.HasValue &&
                                u.fecha_otp_generado.Value <= limite)
                    .ToList();

                if (!usuarios.Any()) return;

                foreach (var usuario in usuarios)
                {
                    usuario.intentos_fallidos = 0;
                }
                dc.SubmitChanges();
            }
        }

        private void DB_GuardarOtp(int id, string hash, DateTime gen)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.codigo_otp = hash;
                usuario.fecha_otp_generado = gen;
                dc.SubmitChanges();
            }
        }

        private void DB_InvalidarOtpPorUsuario(int id)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.codigo_otp = null;
                dc.SubmitChanges();
            }
        }

        private void DB_GuardarClaveTemporal(int id, string hash)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.contraseña = hash;
                usuario.clave_temporal = true;
                dc.SubmitChanges();
            }
        }

        private void DB_ActualizarContrasena(int id, string hash)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var usuario = dc.tbl_usuario.FirstOrDefault(u => u.usu_id == id);
                if (usuario == null) return;
                usuario.contraseña = hash;
                usuario.clave_temporal = false;
                dc.SubmitChanges();
            }
        }

        public tbl_usuario ObtenerDatosUsuario(int usuId)
        {
            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                return dc.tbl_usuario
                    .Where(u => u.usu_id == usuId)
                    .Select(u => new tbl_usuario
                    {
                        usu_id = u.usu_id,
                        usu_nombres = u.usu_nombres,
                        usu_nickname = u.usu_nickname,
                        correo_electronico = u.correo_electronico,
                        rol_id = u.rol_id
                    })
                    .FirstOrDefault();
            }
        }
    }
}
