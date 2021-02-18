using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutomationUtils.Extensions;
using Autotest.Providers;

namespace Autotest.Utils
{
    public class FileSystemHelper
    {
        public static void EnsureFolderExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static string GetPathForScreenshot(string testName)
        {
            var date = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            var fileName = $"{testName}_{date}.png";

            return Path.Combine(PathProvider.ScreenshotsFolder, fileName);
        }

        public enum DataFolder
        {
            TestData,
            Resources
        }

        public static string GeneratePathToEmbeddedResource(string pathPart, DataFolder dataFolder)
        {
            if (string.IsNullOrEmpty(pathPart))
            {
                throw new Exception("Path not set");
            }

            string executingAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathParts = new List<string>() { executingAssemblyFolder, dataFolder.GetValue() };
            pathParts.AddRange(pathPart.Split('\\'));
            var fullPath = Path.Combine(pathParts.ToArray());

            return fullPath;
        }

        public static IList<T> ReadJsonListFromSystem<T>(string pathToJson)
        {
            var fullPath = FileSystemHelper.GeneratePathToEmbeddedResource(pathToJson, DataFolder.TestData);
            var reader = new StreamReader(fullPath);
            string myJson = reader.ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(myJson);
        }

        public static object ReadJsonFromSystem<T>(string pathToJson)
        {
            var fullPath = FileSystemHelper.GeneratePathToEmbeddedResource(pathToJson, DataFolder.TestData);
            var reader = new StreamReader(fullPath);
            string myJson = reader.ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(myJson);
        }
    }
}
