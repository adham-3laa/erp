using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace erp.ViewModels.Returns
{
    public abstract class BaseReturnsViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return;

            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
