using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLOImportTripQuoteInstallerLib
{
    public class InstallerHelper
    {
        #region Variables

        private string PluginName = "HLOImportTripQuote";

        #endregion

        #region Properties
        public string InstallationPath
        {
            get;
            set;
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public InstallerHelper()
        {

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets new SP 7.2 based MP path.
        /// </summary>
        /// <returns></returns>
        private string OtherSmartpointPath()
        {
            string spPath = string.Empty;

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Travelport\Smartpoint\Core";

            if (Directory.Exists(folderPath))
            {
                spPath = folderPath;
            }

            return spPath;
        }

        /// <summary>
        /// Finds Smartpoint install Path
        /// </summary>
        /// <returns></returns>
        private string FindSmartpointPath()
        {
            string sSmartpointPath = string.Empty;
            try
            {
                string spExePath = SmartpointHelper.GetSmartpointPath();
                FileInfo exeFile = new FileInfo(spExePath);

                if (exeFile.Exists)
                {
                    sSmartpointPath = exeFile.Directory.FullName;
                }
            }
            catch (Exception ex)
            {
                throw ex;   //added by Pavan on 06-OCT, 2015
            }

            return sSmartpointPath;

        }
        /// <summary>
        /// Get stream content.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="MAX_STR_LEN"></param>
        /// <returns></returns>
        private string GetStreamContent(string FilePath, int MAX_STR_LEN)
        {
            string line = string.Empty;

            using (StreamReader sr = new StreamReader(FilePath))
            {
                line = sr.ReadToEnd();
                sr.Close();
            }

            return line;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// We add plugin location & reset plugin cache XML
        /// </summary>
        public void Install(string FilePath)
        {
            InstallationPath = FilePath;
            string spPath = FindSmartpointPath();
            string otherSpPath = OtherSmartpointPath();
            string pluginLocation = Path.Combine(spPath, "PluginLocations");
            string otherSPLocation = otherSpPath + "\\PluginLocations";

            if (Directory.Exists(pluginLocation))
            {
                File.Copy(Path.Combine(InstallationPath, string.Format("{0}.xml", PluginName)), Path.Combine(pluginLocation, string.Format("{0}.xml", PluginName)), true);

                string content = GetStreamContent(Path.Combine(pluginLocation, string.Format("{0}.xml", PluginName)), 1024);

                try
                {
                    content = content.Replace("PLACEHOLDER_TEXT", InstallationPath);

                    StreamWriter wr = new StreamWriter(Path.Combine(pluginLocation, string.Format("{0}.xml", PluginName)));
                    wr.Write(content);
                    wr.Close();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (Directory.Exists(otherSPLocation))
            {
                File.Copy(Path.Combine(InstallationPath, string.Format("{0}.xml", PluginName)), Path.Combine(otherSPLocation, string.Format("{0}.xml", PluginName)), true);

                string content = GetStreamContent(Path.Combine(otherSPLocation, string.Format("{0}.xml", PluginName)), 1024);

                try
                {
                    content = content.Replace("PLACEHOLDER_TEXT", InstallationPath);

                    StreamWriter wr = new StreamWriter(Path.Combine(otherSPLocation, string.Format("{0}.xml", PluginName)));
                    wr.Write(content);
                    wr.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (Directory.Exists(pluginLocation) || Directory.Exists(otherSPLocation))
            {
                var installationpath = Path.Combine(InstallationPath, string.Format("{0}.xml", PluginName));
                var installationpathfile = new FileInfo(installationpath);
                installationpathfile.IsReadOnly = false;
                installationpathfile.Delete();

                SmartpointHelper.DeletePluginCache();
            }
        }

        /// <summary>
        /// We move plugin location
        /// </summary>
        public void UnInstall()
        {
            string spPath = FindSmartpointPath();
            string otherSpPath = OtherSmartpointPath();
            string pluginLocation = Path.Combine(spPath, "PluginLocations");
            string otherSPLocation = otherSpPath + "\\PluginLocations";

            if (Directory.Exists(pluginLocation))
            {
                var pluginFile = new FileInfo(Path.Combine(pluginLocation, string.Format("{0}.xml", PluginName)));
                pluginFile.IsReadOnly = false;
                pluginFile.Delete();
            }

            if (Directory.Exists(otherSPLocation))
            {
                var pluginFile = new FileInfo(Path.Combine(otherSPLocation, string.Format("{0}.xml", PluginName)));
                pluginFile.IsReadOnly = false;
                pluginFile.Delete();
            }

            SmartpointHelper.DeletePluginCache();
        }

        #endregion
    }
}
