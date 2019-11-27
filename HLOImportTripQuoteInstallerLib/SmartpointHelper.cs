using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLOImportTripQuoteInstallerLib
{
    internal static class SmartpointHelper
    {
        /// <summary>
        /// Is Smartpoint already installed
        /// </summary>
        /// <returns></returns>
        public static bool IsSmartPointInstalled()
        {
            bool installed = !string.IsNullOrEmpty(GetSmartpointPath());

            return installed;
        }

        /// <summary>
        /// Is Smartpoint running currently
        /// </summary>
        /// <returns></returns>
        public static bool IsSmartpointRunning()
        {
            var processList = Process.GetProcessesByName("Travelport.Smartpoint.App");

            if (processList != null && processList.Any())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Start Smartpoint
        /// </summary>
        public static void StarSmartpoint()
        {
            string path = GetSmartpointPath();

            Process.Start(path);
        }

        /// <summary>
        /// Kill active session of Smartpoint
        /// </summary>
        public static void KillSmartpoint()
        {
            var p = Process.GetProcessesByName("Travelport.Smartpoint.App");

            if (p.Any())
            {
                Process process = (Process)p.First();
                process.Kill();
            }
        }

        /// <summary>
        /// Delelete plugin cache XML
        /// </summary>
        /// <returns></returns>
        public static bool DeletePluginCache()
        {
            bool deleted = false;

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullPath = string.Format("{0}\\Travelport\\Smartpoint\\CachedPlugins.xml", appDataFolder);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    deleted = true;
                }

            }
            catch
            {

            }

            return deleted;
        }

        /// <summary>
        /// Get Smartpoint installed folder
        /// </summary>
        /// <returns></returns>
        public static string GetSmartpointPath()
        {
            string path = "";

            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Travelport\\Smartpoint", false);

            if (key == null)
            {
                key = Registry.LocalMachine.OpenSubKey("Software\\Travelport\\Smartpoint", false);
            }

            if (key != null)
            {
                string installDir = key.GetValue("SmartpointDIR").ToString();

                if (Directory.Exists(installDir))
                {
                    string filePath = String.Format("{0}\\{1}", installDir, "Travelport.Smartpoint.App.exe");

                    if (File.Exists(filePath))
                    {
                        path = filePath;
                    }
                }

            }

            return path;
        }
    }
}
