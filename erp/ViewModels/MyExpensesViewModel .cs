using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;

namespace erp.ViewModels
{
    public class MyExpensesViewModel : INotifyPropertyChanged
    {
        private readonly ExpenseService _expenseService;

        public ObservableCollection<ExpenseResponseDto> Expenses { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public IAsyncRelayCommand LoadExpensesCommand { get; }

        public MyExpensesViewModel()
        {
            _expenseService = new ExpenseService();
            LoadExpensesCommand = new AsyncRelayCommand(LoadExpenses);
            _ = LoadExpenses();
        }

        private async Task LoadExpenses()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Expenses.Clear();

                var data = await _expenseService.GetMyExpenses();

                if (data == null || data.Count == 0)
                {
                    ErrorMessage = "لا توجد مصروفات للعرض";
                    return;
                }

                foreach (var item in data)
                    Expenses.Add(item);
            }
            catch
            {
                ErrorMessage = "حدث خطأ أثناء تحميل مصروفاتك";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
