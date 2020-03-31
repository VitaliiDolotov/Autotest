using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Autotest.Utils
{
    public class FileSystemHelper
    {
        public static void EnsureScreensotsFolderExists()
        {
            var folder = GetScreenshotFolder();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static string GetPathForScreenshot(string testName, string customText = "")
        {
            testName = testName.Replace("\\", string.Empty).Replace("/", string.Empty).Replace("\"", "'");
            var date = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            var fileName = string.IsNullOrEmpty(customText) ?
                $"{testName}_{date}.png" : $"{testName}_{customText}_{date}.png";

            return Path.Combine(GetScreenshotFolder(), fileName);
        }

        private static string GetScreenshotFolder()
        {
            return ConfigurationReader.AppSettings("screenshotsFolder");
        }

        public static string GetPathForDownloadsFolder()
        {
            var downloadFolder = ConfigurationReader.AppSettings("downloadsFolder");
            return downloadFolder.Equals("DEFAULT_DOWNLOADS_FOLDER")
                ? Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Downloads")
                : downloadFolder;
        }

        public static string GeneratePathToEmbeddedResource(string folderName, string embeddedResource)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new Exception("Folder Name is not set");

            if (string.IsNullOrEmpty(embeddedResource))
                throw new Exception("Embedded resource Name is not set");

            var executingAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (executingAssemblyFolder == null)
                throw new Exception("Unable to get path to the executing assembly");

            var filePaths = Directory.GetFiles(Path.Combine(executingAssemblyFolder, folderName), embeddedResource,
                SearchOption.AllDirectories);

            return filePaths.First();
        }

        public static string GeneratePathToEmbeddedResource(string folderName, string shapeImage, string extension)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new Exception("Extension is not set");

            return GeneratePathToEmbeddedResource(folderName, $"{shapeImage}.{extension}");
        }

        public static bool IsFileDownloaded(string fileName)
        {
            var filePath = Path.Combine(FileSystemHelper.GetPathForDownloadsFolder(), fileName);

            for (int i = 0; i < 30; i++)
            {
                if (!File.Exists(filePath))
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    return true;
                }
            }

            Logger.Write($"Downloaded image was not found: {filePath}");
            return false;
        }
    }
}
