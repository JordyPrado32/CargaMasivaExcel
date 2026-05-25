using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Data;

namespace Capa_Negocios
{
    public enum ResultadoLogin { Exitoso, ExitosoClaveTemporalActiva, CredencialesInvalidas, UsuarioBloqueado, ErrorInterno }
    public enum ResultadoRecuperacion { Exitoso, UsuarioNoEncontrado, ErrorInterno }

    public class UsuarioService
    {
        private string cadena = "Data Source=.;Initial Catalog=deberes_4to;User ID=clase4b;Password=clase4b;Encrypt=False;";

        private static string SmtpHost => AppSettingsSecrets.GetRequiredString("Smtp", "Host");
        private static int SmtpPuerto => AppSettingsSecrets.GetRequiredInt("Smtp", "Port");
        private static string SmtpUsuario => AppSettingsSecrets.GetRequiredString("Smtp", "Username");
        private static string SmtpPassword => AppSettingsSecrets.GetRequiredString("Smtp", "Password");
        private const string RemitenteNombre = "Sistema Star Dash";

        private static string TwilioAccountSid => AppSettingsSecrets.GetRequiredString("Twilio", "AccountSid");
        private static string TwilioAuthToken => AppSettingsSecrets.GetRequiredString("Twilio", "AuthToken");
        private static string TwilioNumero => AppSettingsSecrets.GetRequiredString("Twilio", "FromNumber");

        public ResultadoLogin IniciarSesion(string nick, string passwordIngresada, out int rolId, out string correo, out string mensajeErr)
        {
            rolId = 0; correo = string.Empty; mensajeErr = string.Empty;
            try
            {
                DB_ResetearIntentosCaducados();
                var dt = DB_ObtenerUsuarioPorNick(nick);
                if (dt.Rows.Count == 0) { mensajeErr = "Usuario no encontrado."; return ResultadoLogin.CredencialesInvalidas; }

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
                string celular = row.Table.Columns.Contains("numero_celular") ? row["numero_celular"].ToString() : "";

                if (Convert.ToBoolean(row["clave_temporal"])) return ResultadoLogin.ExitosoClaveTemporalActiva;

                string otp = GenerarOtp();
                DB_GuardarOtp(usuId, BCrypt.Net.BCrypt.HashPassword(otp), DateTime.Now);

                string nombreCorto = ObtenerPrimerNombreApellido(nombre);
                try { EnviarAccesoMfaPorCorreo(correo, nombreCorto, otp); } catch { }
                if (!string.IsNullOrEmpty(celular)) { try { EnviarOtpPorWhatsApp(celular, nombreCorto, otp); } catch { } }

                return ResultadoLogin.Exitoso;
            }
            catch (Exception ex) { mensajeErr = ex.Message; return ResultadoLogin.ErrorInterno; }
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
                if (dt.Rows.Count == 0) { mensajeErr = "No encontrado."; return ResultadoRecuperacion.UsuarioNoEncontrado; }

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
            catch (Exception ex) { mensajeErr = ex.Message; return ResultadoRecuperacion.ErrorInterno; }
        }

        public void CambiarContrasena(int usuId, string nuevaClave)
        {
            DB_ActualizarContrasena(usuId, BCrypt.Net.BCrypt.HashPassword(nuevaClave));
        }

        public DataTable ObtenerEstadoCuentas() { return DB_ObtenerEstadoCuentas(); }

        public void ResetearIntentosUsuario(int usuId) { DB_ResetearIntentosAdmin(usuId); }

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

        // ══════════════════════════════════════════════════════════════
        // MÉTODOS DE ENVÍO Y AUXILIARES
        // ══════════════════════════════════════════════════════════════
        private void EnviarAccesoMfaPorCorreo(string destinatario, string nombre, string otp)
        {
            string urlQr = "https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=" + Uri.EscapeDataString("OTP:" + otp);
            string cuerpo = $"<html><body><h2>Verificación</h2><img src='{urlQr}'/><br><h1>{otp}</h1></body></html>";
            using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto) { Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword), EnableSsl = true })
            { smtp.Send(new MailMessage(SmtpUsuario, destinatario, "Acceso Star Dash", cuerpo) { IsBodyHtml = true }); }
        }

        private void EnviarClaveTemporalPorCorreo(string destinatario, string nombre, string claveTemporal)
        {
            string cuerpo = $"<html><body><h2>Recuperación</h2><h1>{claveTemporal}</h1></body></html>";
            using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto) { Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword), EnableSsl = true })
            { smtp.Send(new MailMessage(SmtpUsuario, destinatario, "Clave Temporal", cuerpo) { IsBodyHtml = true }); }
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
            byte[] data = new byte[8]; using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(data);
            return new string(data.Select(b => "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[b % 32]).ToArray());
        }
        private string GenerarClaveTemporal()
        {
            byte[] data = new byte[10]; using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(data);
            return new string(data.Select(b => "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789"[b % 54]).ToArray());
        }
        private string ObtenerPrimerNombreApellido(string completo)
        {
            var p = completo.Trim().Split(' '); return p.Length >= 3 ? $"{p[0]} {p[2]}" : p[0];
        }

        private DataTable ExecQuery(string q, params SqlParameter[] p)
        {
            using (var con = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(q, con))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                var da = new SqlDataAdapter(cmd); var dt = new DataTable();
                con.Open(); da.Fill(dt); return dt;
            }
        }
        public int ObtenerIdPorNick(string nick)
        {
            var dt = DB_ObtenerUsuarioPorNick(nick);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["usu_id"]) : 0;
        }
        private void ExecNonQuery(string q, params SqlParameter[] p)
        {
            using (var con = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(q, con))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                con.Open(); cmd.ExecuteNonQuery();
            }
        }

        private DataTable DB_ObtenerUsuarioPorNick(string nick) => ExecQuery("SELECT usu_id, contraseña, rol_id, intentos_fallidos, correo_electronico, usu_nombres, clave_temporal, numero_celular FROM tbl_usuario WHERE usu_nickname = @n OR correo_electronico = @n", new SqlParameter("@n", nick));
        private DataTable DB_ObtenerUsuarioPorNickOCorreo(string valor) => ExecQuery("SELECT usu_id, usu_nombres, numero_celular, correo_electronico FROM tbl_usuario WHERE usu_nickname = @v OR correo_electronico = @v", new SqlParameter("@v", valor));
        private DataTable DB_ObtenerEstadoCuentas() => ExecQuery("SELECT usu_id, usu_nombres, usu_nickname, correo_electronico, intentos_fallidos, estado_cuenta, ultimo_intento, rol_id FROM vw_EstadoCuentas ORDER BY rol_id, usu_nombres");
        private DataTable DB_ObtenerUsuarioPorId(int id) => ExecQuery("SELECT usu_id, usu_nombres, usu_nickname, correo_electronico, numero_celular, intentos_fallidos, rol_id, clave_temporal FROM tbl_usuario WHERE usu_id = @id", new SqlParameter("@id", id));
        private DataTable DB_ObtenerOtpVigente() => ExecQuery("SELECT usu_id, rol_id, codigo_otp FROM tbl_usuario WHERE codigo_otp IS NOT NULL AND DATEDIFF(MINUTE, fecha_otp_generado, GETDATE()) <= 5");

        private void DB_IncrementarIntentosFallidos(int id) => ExecNonQuery("UPDATE tbl_usuario SET intentos_fallidos = intentos_fallidos + 1 WHERE usu_id = @id", new SqlParameter("@id", id));
        private void DB_ResetearIntentos(int id) => ExecNonQuery("UPDATE tbl_usuario SET intentos_fallidos = 0 WHERE usu_id = @id", new SqlParameter("@id", id));
        private void DB_ResetearIntentosAdmin(int id) => ExecNonQuery("UPDATE tbl_usuario SET intentos_fallidos = 0, codigo_otp = NULL, fecha_otp_generado = NULL WHERE usu_id = @id", new SqlParameter("@id", id));
        private void DB_ResetearIntentosCaducados() => ExecNonQuery("UPDATE tbl_usuario SET intentos_fallidos = 0 WHERE intentos_fallidos > 0 AND intentos_fallidos <= 2 AND DATEDIFF(HOUR, fecha_otp_generado, GETDATE()) >= 24");
        private void DB_GuardarOtp(int id, string hash, DateTime gen) => ExecNonQuery("UPDATE tbl_usuario SET codigo_otp = @o, fecha_otp_generado = @g WHERE usu_id = @id", new SqlParameter("@o", hash), new SqlParameter("@g", gen), new SqlParameter("@id", id));
        private void DB_InvalidarOtpPorUsuario(int id) => ExecNonQuery("UPDATE tbl_usuario SET codigo_otp = NULL WHERE usu_id = @id", new SqlParameter("@id", id));
        private void DB_GuardarClaveTemporal(int id, string hash) => ExecNonQuery("UPDATE tbl_usuario SET contraseña = @h, clave_temporal = 1 WHERE usu_id = @id", new SqlParameter("@h", hash), new SqlParameter("@id", id));
        private void DB_ActualizarContrasena(int id, string hash) => ExecNonQuery("UPDATE tbl_usuario SET contraseña = @h, clave_temporal = 0 WHERE usu_id = @id", new SqlParameter("@h", hash), new SqlParameter("@id", id));
    
    public tbl_usuario ObtenerDatosUsuario(int usuId)
        {
            var dc = new Capa_Datos.MonolitoDataContext();

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