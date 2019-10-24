// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="Name of the company">
//   Copyright (c).  All rights reserved.
// </copyright>
// <summary>
//   Helper methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImportTripQuotePlugin.Common
{
    using Travelport.Smartpoint.Controls;
    using Travelport.Smartpoint.Helpers.Core;
    using Travelport.Smartpoint.Helpers.UI;
    using Travelport.TravelData;
    using Travelport.TravelData.Factory;

    /// <summary>
    /// Helper methods
    /// </summary>
    internal static class Helper
    {
        #region Desktop Factory
        /// <summary>
        /// Get the Galileo Desktop Factory
        /// </summary>
        /// <returns>Returns the factory</returns>
        public static GalileoDesktopFactory GetDesktopFactory()
        {
            var factory = UIHelper.Instance.CurrentTEControl.Connection.CommunicationFactory as GalileoDesktopFactory;
            return factory;
        }

        /// <summary>
        /// Get the current booking file from Smart point
        /// </summary>
        /// <returns>Returns the current booking file, if found, else null</returns>
        public static BookingFile GetBookingFile()
        {
            BookingFile bookingFile = null;

            // Retrieve Current TE Control
            TEControl currentTeControl = UIHelper.Instance.CurrentTEControl;

            // Get the GalileoDesktopFactory associated with the current TEControl.
            var galileoDesktopFactory = currentTeControl.Connection.CommunicationFactory as GalileoDesktopFactory;

            if (galileoDesktopFactory != null)
            {
                bookingFile = galileoDesktopFactory.RetrieveCurrentBookingFile();
            }

            return bookingFile;
        }
        #endregion

        #region Terminal output
        /// <summary>
        /// Send output to the terminal window
        /// </summary>
        /// <param name="msg">The message text to send to the terminal</param>
        /// <param name="addSOM">If should add Start of Message, default true</param>
        public static void SendTerminalOutput(string msg, bool addSOM = true)
        {
            UIHelper.Instance.AddMessageInTerminal(msg, UIHelper.Instance.CurrentTEControl, addSOM);
        }

        /// <summary>
        /// Send a command to Smart point to execute
        /// </summary>
        /// <param name="command">The command line to send to Smart point</param>
        /// <param name="showInTE">If Smart point should display the command in the output window, default true</param>
        public static void SendTerminalCommand(string command, bool showInTE = true)
        {
            // Sends the command to Host
            CoreHelper.Instance.SendHostCommand(command, UIHelper.Instance.CurrentTEControl, showInTE);
        }

        /// <summary>
        /// Display a message in the popup window
        /// </summary>
        /// <param name="msg">The message to display</param>
        /// <param name="seconds">The number of seconds to display, default 10</param>
        public static void ShowPopup(string msg, int seconds = 10)
        {
            UIHelper.Instance.ShowPopup(msg, seconds);
        }
        #endregion
    }
}