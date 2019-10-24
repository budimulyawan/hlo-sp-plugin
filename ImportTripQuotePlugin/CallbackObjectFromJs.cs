using ImportTripQuotePlugin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportTripQuotePlugin
{
    public class CallbackObjectFromJs
    {
        public void sendTerminalCommand(string command)
        {
            Helper.SendTerminalCommand(command);
        }
    }
}
