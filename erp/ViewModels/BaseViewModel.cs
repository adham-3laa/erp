//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using CommunityToolkit.Mvvm.ComponentModel;




using System.ComponentModel;
using System.Runtime.CompilerServices;

//namespace erp.ViewModels;

//public abstract class BaseViewModel : INotifyPropertyChanged
//{
//    public event PropertyChangedEventHandler? PropertyChanged;

//    protected void OnPropertyChanged([CallerMemberName] string? name = null)
//        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

//    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
//    {
//        if (Equals(field, value)) return false;
//        field = value;
//        OnPropertyChanged(name);
//        return true;
//    }
//}
using CommunityToolkit.Mvvm.ComponentModel;

namespace erp.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

