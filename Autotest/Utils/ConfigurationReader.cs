using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SfsExtras.Utils
{
    public class ConfigurationReader
    {
        public static string JsonAppSettings(string key)
        {
            try
            {
                string executingAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.Combine(executingAssemblyFolder, "appsettings.json");

                var r = new StreamReader(path);
                string json = r.ReadToEnd();
                JObject settingsJson = JObject.Parse(json);

                JObject application = (JObject)settingsJson["appSettings"];
                return application[key].ToString();
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read '{key}' property from json config file: {e}");
            }
        }

        public static string AppConfigSettings(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read '{key}' property from config file: {e}");
            }
        }

        public static string AppSettings(string key)
        {
            var fromAppConfig = string.Empty;
            var fromJsonAppConfig = string.Empty;

            try
            {
                fromAppConfig = AppConfigSettings(key);
            }
            catch { }

            try
            {
                fromJsonAppConfig = JsonAppSettings(key);
            }
            catch { }

            return !string.IsNullOrEmpty(fromAppConfig) ? fromAppConfig : fromJsonAppConfig;
        }
    }
}
