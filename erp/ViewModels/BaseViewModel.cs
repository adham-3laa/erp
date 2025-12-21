using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace erp.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
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

//namespace erp.ViewModels
//{
//    public abstract class BaseViewModel : INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged;

//        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }

//        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
//        {
//            if (Equals(field, value)) return false;
//            field = value;
//            OnPropertyChanged(propertyName);
//            return true;
//        }
//    }
//}

