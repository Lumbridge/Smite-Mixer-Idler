using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Smite.Mixer.Idler.Helpers
{
    public static class UiHelper
    {
        // updates the main UI and taskbar launch with windows entry to reflect currently selected option
        public static void UpdateUiAndTaskbarIcon(MainWindow mw, MenuItem mi, bool trueFalse)
        {
            mw?.LaunchWithWindows.Dispatcher.BeginInvoke((Action)(() => mw.LaunchWithWindows.Text = trueFalse.ToString()));
            mi.Header = $"_Start Smite Mixer Idler with Windows [{trueFalse.ToString()}]";
        }
    }
}
