using System.Windows;
using System.Windows.Input;

namespace Smite.Mixer.Idler.Commands
{
    class ShowWindowCommand : CommandBase<ShowWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Show();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null && !win.IsVisible;
        }
    }
}