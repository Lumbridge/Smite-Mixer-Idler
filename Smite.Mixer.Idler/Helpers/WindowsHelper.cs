using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

namespace Smite.Mixer.Idler.Helpers
{
    public class WindowsHelper
    {
        public static bool ToggleLaunchWithWindows(object parameter)
        {
            var reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var x = parameter as TaskbarIcon;

            if (reg != null)
            {
                try
                {
                    var currentlyEnabledAtStartup = reg.GetValue("SmiteMixerIdler") != null;

                    if (currentlyEnabledAtStartup)
                    {
                        reg.DeleteValue("SmiteMixerIdler");
                        x?.CloseBalloon();
                        x?.ShowBalloonTip("Smite Mixer Idler", "Smite Mixer Idler will not launch with Windows.", BalloonIcon.Info);
                    }
                    else
                    {
                        reg.SetValue("SmiteMixerIdler", System.Reflection.Assembly.GetExecutingAssembly().Location);
                        x?.CloseBalloon();
                        x?.ShowBalloonTip("Smite Mixer Idler", "Smite Mixer Idler will launch with Windows.", BalloonIcon.Info);
                    }
                }
                catch
                {
                    x?.ShowBalloonTip("Smite Mixer Idler", "Failed to Get current launch with Windows parameter from Registry.", BalloonIcon.Error);
                }
            }
            else
            {
                x?.ShowBalloonTip("Smite Mixer Idler", "Couldn't access user registry.", BalloonIcon.Error);
            }
        }
    }
}
