using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Action<object?>? _executeWithParam;
    private readonly Func<bool>? _canExecute;
    private readonly Func<object?, bool>? _canExecuteWithParam;

    // ✅ Constructor for parameterless methods: () => ...
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // ✅ Constructor for methods with parameter: (obj) => ...
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecuteWithParam = canExecute;
        _execute = () => _executeWithParam(null);
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecuteWithParam != null)
            return _canExecuteWithParam(parameter);

        return _canExecute == null || _canExecute();
    }

    public void Execute(object? parameter)
    {
        if (_executeWithParam != null)
            _executeWithParam(parameter);
        else
            _execute();
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // ✅ optional helper to refresh CanExecute manually
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

/// <summary>
/// ✅ Generic RelayCommand if you want strongly-typed parameters later.
/// Example: new RelayCommand<int>(id => ..., id => id > 0)
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;

        if (parameter is T t) return _canExecute(t);

        // لو parameter null و T reference type / nullable
        return _canExecute(default);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T t) _execute(t);
        else _execute(default);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
