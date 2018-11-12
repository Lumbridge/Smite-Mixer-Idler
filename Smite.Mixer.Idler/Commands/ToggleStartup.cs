using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Smite.Mixer.Idler.Helpers;

namespace Smite.Mixer.Idler.Commands
{
    class ToggleStartup : CommandBase<ToggleStartup>
    {
        public override void Execute(object parameter)
        {
            var mainIcon = parameter as TaskbarIcon;
            var mainWindow = GetTaskbarWindow(mainIcon) as MainWindow;
            var menuitem = mainWindow?.MainIcon?.ContextMenu?.Items[0] as MenuItem;

            var launchWithWindows = WindowsHelper.ToggleStartup();

            if (launchWithWindows != null)
            {
                UiHelper.UpdateUiAndTaskbarIcon(mainWindow, menuitem, (bool)launchWithWindows);
            }
            else
            {
                mainIcon?.ShowBalloonTip("Smite Mixer Idler", "An error occurred when trying to interact with the registry, unable to set launch with windows setting.", BalloonIcon.Info);
            }
        }
    }
}
