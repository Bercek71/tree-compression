using System.Windows.Input;

namespace TreeCompressionMain;

public abstract class BaseCommand : ICommand
{
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public abstract void Execute(object? parameter);

    public event EventHandler? CanExecuteChanged;
}