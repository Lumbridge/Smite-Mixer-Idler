using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Smite.Mixer.Idler.Helpers
{
    /// <summary>
    /// A helper for windows related stuff e.g. registry interaction.
    /// </summary>
    public static class WindowsHelper
    {
        // a static accessor for a commonly used registry entry location
        public static RegistryKey Reg => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        // returns the current launch with windows setting (does it exist in the registry)
        public static bool? GetStartup()
        {
            // check that we have access to the registry
            if (Reg == null)
                return null;
            // if it exists in the registry then the option is true else false
            return Reg.GetValue("SmiteMixerIdler") != null;
        }

        // sets the launch with windows option
        public static bool? SetStartup(bool launchWithWindows)
        {
            // check that we have access to the registry
            if (Reg == null)
                return null;

            try
            {
                if (launchWithWindows)
                {
                    // add the launch with windows item to the registry
                    Reg.SetValue("SmiteMixerIdler", Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    // remove the launch with windows item from the registry
                    Reg.DeleteValue("SmiteMixerIdler");
                }
                // return true as success
                return true;
            }
            catch
            {
                // return null because something went wrong
                return null;
            }
        }

        // toggles the launch with windows option
        public static bool? ToggleStartup()
        {
            // check that we have access to the registry
            if (Reg == null)
                return null;

            // get the current launch with windows option
            var launchWithWindows = GetStartup();

            // toggle the current launch with windows option
            if (launchWithWindows != null)
            {
                SetStartup(!(bool)launchWithWindows);
            }

            // return the current launch with windows option
            return GetStartup();
        }
    }
}
