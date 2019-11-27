// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Plugin.cs" company="Name of the company">
//  Copyright (c). All rights reserved.
// </copyright>
// <summary>
//   Plugin for Travelport smartpoint
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImportTripQuotePlugin
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Threading;
    using Travelport.MvvmHelper;
    using Travelport.Smartpoint.Common;
    using Travelport.Smartpoint.Controls;
    using Travelport.Smartpoint.Helpers.Core;
    using Travelport.Smartpoint.Helpers.UI;
    using Travelport.Smartpoint.Plugins.Wishlist.Converters;
    using Travelport.Smartpoint.Plugins.Wishlist.Helpers;
    using Travelport.Smartpoint.Plugins.Wishlist.ShopFlightOption.ViewModels;
    using Travelport.Smartpoint.Plugins.Wishlist.ViewModels;

    /// <summary>
    /// Plugin for extending Smartpoint application
    /// </summary>
    [SmartPointPlugin(Developer = "Name of the developer",
                      Company = "Name of the Company",
                      Description = "Small description about the plugin",
                      Version = "Version number of the plugin",
                      Build = "Build version of Smartpoint application")]
    public class Plugin : HostPlugin
    {
        /// <summary>
        /// Handles the terminal input and output
        /// </summary>

        /// <summary>
        /// Executes the load actions for the plugin.
        /// </summary>
        public override void Execute()
        {
            // Attach a handler to execute when the Smartpoint application is ready and all windows are loaded.
            CoreHelper.Instance.OnSmartpointReady += this.OnSmartpointReady;
        }

        #region Handlers

        /// <summary>
        /// Handles the Smartpoint Ready event of the Instance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Travelport.Smartpoint.Helpers.Core.CoreHelperEventArgs"/> instance containing the event data.</param>
        private void OnSmartpointReady(object sender, CoreHelperEventArgs e)
        {
            //UIHelper.Instance.ShowMessageBox("Start Hello");
            // Hook into any terminal commands we are interested in
            // Commands entered by the user before they are executed
            CoreHelper.Instance.TerminalCommunication.OnTerminalPreSend += this.OnTerminalPreSend;

            // Output before it is displayed
            CoreHelper.Instance.TerminalCommunication.OnTerminalPreResponse += this.OnTerminalPreResponse;

            addCustomToolbarTerminalItem();
        }

        /// <summary>
        /// Traps plugin specific cryptic entries and process them.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Arguments from the terminal.</param>
        private void OnTerminalPreSend(object sender, TerminalEventArgs e)
        {
        }

        /// <summary>
        /// Have terminal output
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The terminal event arguments</param>
        private void OnTerminalPreResponse(object sender, TerminalEventArgs e)
        {
        }

        internal ISmartBrowserWindow GetSmartBrowserWindowById(string id)
        {
            ISmartBrowserWindow window = null;

            if (UIHelper.Instance.SmartpointWindows.Any(p => p.SmartWindowID == id))
            {
                window = UIHelper.Instance.SmartpointWindows.OfType<ISmartBrowserWindow>().FirstOrDefault(p => p.SmartWindowID == id);
            }

            return window;
        }

        private void addCustomToolbarTerminalItem()
        {
            // Create a SmartCustomToolbarButton
            var button = new SmartCustomToolbarButton();
            button.Style = (Style)button.FindResource("CustomToolbarButton");
            // Add a command
            button.Command = new RelayCommand((a) =>
            {
                if (Boolean.Parse(GetConfigValue("UseTQC")))
                {
                    CoreHelper.Instance.InvokePluginCommand("Copy_TQC_APAC", null, null);
                    var quote = String.Empty;
                    if (!Boolean.Parse(GetConfigValue("CopyToClipboardOnly")))
                    {
                        quote = Clipboard.GetDataObject().GetData(DataFormats.Html) as string;
                    }
                    LoadPortal(quote);
                }
                else
                {
                    CopyFromTQ();
                }
            });

            // Set the default content 
            button.ButtonContentDefault = "HLO";

            // Set the default content 
            button.ButtonContentSelected = button.ButtonContentDefault;

            //Set the Hover button content
            button.ButtonContentHover = new TextBlock(new Bold(new Run("Helloworld Plugin")));

            // Set the tooltip
            button.ToolTip = "Helloworld Plugin";

            button.Content = "HLO";
            UIHelper.Instance.CurrentSmartTerminalWindow.CustomToolbar.Add(button);
        }

        private void CopyFromTQ()
        {
            var errorMessage = String.Empty;
            string format = GetConfigValue("QuoteFormat");
            TextDataFormat textDataFormat = TextDataFormat.UnicodeText;

            QuoteFormat quoteFormat = Travelport.Smartpoint.Plugins.Wishlist.Converters.QuoteFormat.TextFormat;
            if (format.Equals("HTML"))
            {
                quoteFormat = QuoteFormat.HtmlFormat;
                textDataFormat = TextDataFormat.Html;
            }
            var dataObject = Travelport.Smartpoint.Plugins.Wishlist.Helpers.CopyToClipboard.CopyClipBoard(quoteFormat, out errorMessage, true);
            var quote = dataObject.GetText(textDataFormat);
            LoadPortal(quote);
            string s = string.Empty;
        }

        private string GetDTOFromTQ()
        {
            return DtoHelper.ConvertQuotesToJson("test", "123", "test footer", "aircomment", "hotelcomment",
                "carcomment", "railComment", "railcardcomment", "railCardreceiptComment", "en");
        }

        private void LoadPortal(String htmlQuote)
        {
            string Locator = string.Empty;
            var pcc = Common.Helper.GetDesktopFactory().GetCurrentPcc();
            var sessionAreaInformation = Helper.GetDesktopFactory().GetSessionInformation().ActiveArea;
            var bookingfile = Helper.GetDesktopFactory().RetrieveCurrentBookingFile();
            var agentId = sessionAreaInformation.SignOnIdentifier;

            if (!string.IsNullOrEmpty(bookingfile.RecordLocator))
            {
                Locator = bookingfile.RecordLocator.ToString().Trim();
                var url = GetConfigValue("URL");
                var baseUrl = url;
                var testMode = Boolean.Parse(GetConfigValue("TestMode"));

                url += testMode ? String.Empty : "?" + String.Format("sign_on_identifier={0}&pcc={1}", agentId, pcc);


                var window = CreateWebBrowserWindow(new Uri(url), "locomote", "Locomote");
                window.Show();
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;
                window.Title = String.Format("HelloWorld Import Trip Quote v{0}", version);
                var wb = window.Content as SmartBrowserControl;
                //var filteredQuote = FilterOutRules(quote);



                wb.WebBrowserControl.FrameLoadEnd += (sender, eventArgs) =>
                {
                    var htmlElement = GetConfigValue("PasteHTMLElement");
                    var jsonQuote = GetDTOFromTQ();

                    //var js = String.Format("document.querySelector('{0}').value = JSON.stringify({1})", htmlElement, jsonQuote);
                    var jsAgentNo = String.Format("document.getElementById('AgentNo').value = '" + GetConfigValue("AgentNo").ToString() + "'");
                    var jsUserId = String.Format("document.getElementById('UserId').value = '" + GetConfigValue("UserId").ToString() + "'");
                    var jsPassword = String.Format("document.getElementById('Password').value = '" + GetConfigValue("Password").ToString() + "'");
                    var jsClick = String.Format("document.getElementById('LoginBtn').click()");
                    var host = eventArgs.Browser.GetHost();
                    if (host != null && testMode)
                    {
                        host.ShowDevTools();
                    }

                    //Wait for the MainFrame to finish loading
                    if (eventArgs.Frame.IsMain)
                    {
                        eventArgs.Frame.ExecuteJavaScriptAsync(jsAgentNo);
                        eventArgs.Frame.ExecuteJavaScriptAsync(jsUserId);
                        eventArgs.Frame.ExecuteJavaScriptAsync(jsPassword);
                        eventArgs.Frame.ExecuteJavaScriptAsync(jsClick);
                    }
                };

                if (wb.WebBrowserControl.IsLoaded)
                {

                    var PNRURL = GetConfigValue("PNRURL");
                    var testModePNR = Boolean.Parse(GetConfigValue("TestMode"));

                    //PNRURL += testModePNR ? String.Empty : "?" + String.Format("sign_on_identifier={0}&pcc={1}", agentId, pcc);

                    var PNRwindow = CreateWebBrowserWindow(new Uri(PNRURL), "locomote", "Locomote");
                    PNRwindow.Show();
                    System.Reflection.Assembly PNRassembly = System.Reflection.Assembly.GetExecutingAssembly();
                    FileVersionInfo PNRfvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string PNRversion = fvi.FileVersion;
                    PNRwindow.Title = String.Format("HelloWorld Import Trip Quote v{0}", PNRversion);
                    var PNRwb = PNRwindow.Content as SmartBrowserControl;
                    //var filteredQuote = FilterOutRules(quote);


                    PNRwb.WebBrowserControl.FrameLoadEnd += (sender, eventArgs) =>
                    {
                        var htmlElement = GetConfigValue("PasteHTMLElement");
                        var jsonQuote = GetDTOFromTQ();
                        // you need to put js element .value for usrename password and
                        // .click() down here....

                        var jsAgentNo = String.Format("document.getElementById('AgentNo').value = '" + GetConfigValue("AgentNo").ToString() + "'");

                        var js = String.Format("document.querySelector('{0}').value = JSON.stringify({1})", htmlElement, jsonQuote);
                        var jsPCC = String.Format("document.getElementById('Pcc').value = '" + pcc + "'");
                        var jsTransactionType = String.Format("document.getElementById('TransactionType').value = 'Ticketing'");
                        var jsIsScheduleChange = String.Format("document.getElementById('IsScheduleChange').value = false'");
                        var jsTicketing = String.Format("document.getElementById('selectTicketing').checked = true");
                        var jsPnr = String.Format("document.getElementById('Locator').value = '" + Locator + "'");
                        //var jsSubmitClick = String.Format("document.getElementById('submit').click()");

                        var jsSubmitClick = String.Format("$('button[type=\"submit\"]').click()");
                        var host = eventArgs.Browser.GetHost();
                        if (host != null && testMode)
                        {
                            host.ShowDevTools();
                        }

                        //Wait for the MainFrame to finish loading
                        if (eventArgs.Frame.IsMain)
                        {
                            //eventArgs.Frame.ExecuteJavaScriptAsync(String.Format("console.log('{0}')", js));
                            eventArgs.Frame.ExecuteJavaScriptAsync(jsPCC);
                            eventArgs.Frame.ExecuteJavaScriptAsync(jsTransactionType);
                            eventArgs.Frame.ExecuteJavaScriptAsync(jsIsScheduleChange);
                            eventArgs.Frame.ExecuteJavaScriptAsync(jsTicketing);
                            eventArgs.Frame.ExecuteJavaScriptAsync(jsPnr);
                            if (GetConfigValue("PNRURL") == eventArgs.Frame.Url.ToString().Trim()) // need to delete content after ?
                            {
                                eventArgs.Frame.ExecuteJavaScriptAsync(jsSubmitClick);
                            }
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Create a standard web browser window
        /// </summary>
        /// <param name="url">Url to be opened in the browser</param>
        /// <param name="browserId">Browser identificator</param>
        /// <param name="title">Title to be displayed in the browser header</param>
        /// <returns></returns>
        private ISmartBrowserWindow CreateWebBrowserWindow(Uri url, string browserId, string title)
        {
            if (string.IsNullOrEmpty(browserId))
            {
                browserId = "Browser";
            }

            var browserWindow = UIHelper.Instance.CreateNewSmartBrowserWindow(browserId);
            browserWindow.WindowStyle = new WindowStyle();
            browserWindow.Title = title;
            browserWindow.NoClose = false;
            browserWindow.Width = 1350;
            browserWindow.Height = 750;
            browserWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            browserWindow.Owner = UIHelper.Instance.GetOwnerWindow(UIHelper.Instance.CurrentTEControl.SmartTerminalWindow);

            var wb = browserWindow.Content as SmartBrowserControl;

            if (wb == null)
            {
                wb = new SmartBrowserControl();
                try
                {
                    wb.WebBrowserControl.RegisterJsObject("spHelper", this);
                }
                catch (Exception ex)
                {

                }
                wb.NavigateTo(url);

                browserWindow.Content = wb;

                browserWindow.Closed += delegate
                {
                    wb.Close();
                };
                wb.WebBrowserControl.FrameLoadEnd += delegate
                {
                    if (string.IsNullOrEmpty(title))
                    {
                        wb.WebBrowserControl.Dispatcher.BeginInvoke(
                       DispatcherPriority.Normal,
                       new Action(() => { browserWindow.Title = wb.WebBrowserControl.Title; }));
                    }

                };
            }
            else
            {
                wb.NavigateTo(url);
            }

            return browserWindow;
        }

        public void sendTerminalCommand(string command)
        {
            UIHelper.Instance.CurrentTEControl.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal, new Action(
                    () => { Helper.SendTerminalCommand(command); }
                    ));
        }

        private string GetConfigValue(string key)
        {
            var configuration = GetDllConfiguration(this.GetType().Assembly);
            var section = (System.Configuration.ClientSettingsSection)(configuration.GetSection("applicationSettings/ImportTripQuotePlugin.Properties.Production"));
            return section.Settings.Get(key).Value.ValueXml.LastChild.InnerText.ToString();
        }

        private Configuration GetDllConfiguration(Assembly targetAsm)
        {
            var configFile = targetAsm.Location + ".config";
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFile
            };
            return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        #endregion
    }
}
