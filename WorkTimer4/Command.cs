using System;
using System.Windows.Input;

namespace WorkTimer4
{
    internal class Command : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        public event EventHandler? CanExecuteChanged;

        public Command(Action<object?> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public Command(Action<object?> execute, Func<object?, bool>? canExecute)
            : this(execute)
        {            
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (this.canExecute is null)
            {
                return true;
            }

            return this.canExecute.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            if (!this.CanExecute(parameter))
            {
                return;
            }

            this.execute.Invoke(parameter);
        }
    }
}
