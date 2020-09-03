using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DesktopToggle
{
    class TrayClickCommand : ICommand
    {
        readonly Func<bool> ExecCallback;
        public TrayClickCommand(Func<bool> ExectutionCallback)
        {
            ExecCallback = ExectutionCallback;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (ExecCallback != null)
                return true;
            else
                return false;
        }

        public void Execute(object parameter)
        {
            ExecCallback();
        }
    }
}
