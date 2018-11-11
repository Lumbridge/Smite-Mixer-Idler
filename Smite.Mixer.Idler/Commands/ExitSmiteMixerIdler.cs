using System.Windows;
using System.Windows.Input;

namespace Smite.Mixer.Idler.Commands
{
    class ExitSmiteMixerIdler : CommandBase<ExitSmiteMixerIdler>
    {
        public override void Execute(object parameter)
        {
            Application.Current.Shutdown();
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
