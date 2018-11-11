using System.Windows;
using System.Windows.Input;

namespace Smite.Mixer.Idler.Commands
{
    class HideWindowCommand : CommandBase<HideWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Hide();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null && win.IsVisible;
        }
    }
}