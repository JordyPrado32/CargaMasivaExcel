using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Capa_Negocios
{
    internal static class AppSettingsSecrets
    {
        private static readonly object SyncRoot = new object();
        private static JObject _settings;

        public static string GetRequiredString(string section, string key)
        {
            JObject settings = LoadSettings();
            JToken token = settings.SelectToken(section + "." + key);

            if (token == null || string.IsNullOrWhiteSpace(token.ToString()))
            {
                throw new InvalidOperationException(
                    "Falta la configuración '" + section + ":" + key + "' en appsettings.json.");
            }

            string value = token.ToString().Trim();
            if (string.IsNullOrWhiteSpace(value) ||
                value.StartsWith("REEMPLAZA_", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("TU_", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("tu-app-password") ||
                value.Contains("tu-correo"))
            {
                throw new InvalidOperationException(
                    "La configuración '" + section + ":" + key + "' aún tiene un valor de ejemplo en appsettings.json.");
            }

            return value;
        }

        public static int GetRequiredInt(string section, string key)
        {
            string value = GetRequiredString(section, key);
            int parsedValue;

            if (!int.TryParse(value, out parsedValue))
            {
                throw new InvalidOperationException(
                    "La configuración '" + section + ":" + key + "' no es un número válido.");
            }

            return parsedValue;
        }

        private static JObject LoadSettings()
        {
            if (_settings != null)
            {
                return _settings;
            }

            lock (SyncRoot)
            {
                if (_settings != null)
                {
                    return _settings;
                }

                string[] candidates =
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "appsettings.json"),
                    Path.Combine(Environment.CurrentDirectory, "appsettings.json")
                };

                foreach (string candidate in candidates)
                {
                    string fullPath = Path.GetFullPath(candidate);
                    if (!File.Exists(fullPath))
                    {
                        continue;
                    }

                    string json = File.ReadAllText(fullPath);
                    _settings = JObject.Parse(json);
                    return _settings;
                }

                throw new FileNotFoundException(
                    "No se encontró appsettings.json. Cree el archivo en la raíz del proyecto web Monolito4bm.");
            }
        }
    }
}
