using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Capa_Negocios
{
    public class tbl_usuario
    {
        public int usu_id { get; set; }
        public string usu_nombres { get; set; }
        public string usu_cedula { get; set; }
        public string correo_electronico { get; set; }
        public string contraseña { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string usu_nickname { get; set; }
        public string numero_celular { get; set; }
        public int rol_id { get; set; }
        public DateTime fecha_creacion { get; set; }
        public List<tbl_foto> Fotos { get; set; } = new List<tbl_foto>();

        public int Registrarse(string passSinEncriptar, string n1, string n2, string a1, string a2)
        {
            var regexPass = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$");
            if (!regexPass.IsMatch(passSinEncriptar))
                throw new Exception("La contraseña no cumple la ISO 27001.");

            ValidarNombres(n1, n2, a1, a2);
            if (!ValidarCedulaEcuatoriana(this.usu_cedula)) throw new Exception("Cédula inválida.");
            if (!Regex.IsMatch(this.correo_electronico, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new Exception("Correo inválido.");
            if (this.edad < 18) throw new Exception("Debe ser mayor de 18 años.");

            this.contraseña = BCrypt.Net.BCrypt.HashPassword(passSinEncriptar);
            this.fecha_creacion = DateTime.Now;
            this.usu_nombres = $"{n1} {n2} {a1} {a2}";
            this.usu_nickname = GenerarNickname(n1, a1, a2, n2, this.usu_cedula);

            using (var dc = new Capa_Datos.MonolitoDataContext())
            {
                var nuevoUsuario = new Capa_Datos.tbl_usuario
                {
                    usu_nombres = this.usu_nombres,
                    usu_cedula = this.usu_cedula,
                    correo_electronico = this.correo_electronico,
                    contraseña = this.contraseña,
                    fecha_nacimiento = this.fecha_nacimiento,
                    usu_nickname = this.usu_nickname,
                    numero_celular = this.numero_celular,
                    rol_id = this.rol_id,
                    fecha_creacion = this.fecha_creacion,
                    clave_temporal = false,
                    intentos_fallidos = 0
                };

                dc.tbl_usuario.InsertOnSubmit(nuevoUsuario);
                dc.SubmitChanges();

                return nuevoUsuario.usu_id;
            }
        }

        private void ValidarNombres(string n1, string n2, string a1, string a2)
        {
            if (string.IsNullOrWhiteSpace(n1) || string.IsNullOrWhiteSpace(n2) || string.IsNullOrWhiteSpace(a1) || string.IsNullOrWhiteSpace(a2))
                throw new Exception("Todos los nombres y apellidos son obligatorios.");
        }

        private string GenerarNickname(string n1, string a1, string a2, string n2, string cedula)
        {
            Random rnd = new Random();
            string simbolos = "@#$*&";
            char sim = simbolos[rnd.Next(simbolos.Length)];
            char c1 = n1.Trim()[0];
            char c2 = a1.Trim()[0];
            char c3 = a2.Trim().Length > 1 ? a2.Trim()[1] : a2.Trim()[0];
            char c4 = n2.Trim().Length > 1 ? n2.Trim()[1] : n2.Trim()[0];
            string letras = $"{c1}{c2}{c3}{c4}";
            var mezcladas = letras.Select(c => rnd.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c)).ToArray();
            char num1 = cedula[rnd.Next(cedula.Length)];
            char num2 = cedula[rnd.Next(cedula.Length)];
            return $"{sim}{new string(mezcladas)}{num1}{num2}";
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit)) return false;
            int prov = int.Parse(cedula.Substring(0, 2));
            if (prov < 1 || prov > 24) return false;

            int suma = 0;
            int[] coef = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            for (int i = 0; i < 9; i++)
            {
                int val = int.Parse(cedula[i].ToString()) * coef[i];
                suma += val > 9 ? val - 9 : val;
            }

            int verificador = int.Parse(cedula[9].ToString());
            int calculado = ((suma + 9) / 10 * 10) - suma;
            if (calculado == 10) calculado = 0;
            return calculado == verificador;
        }
    }
}
