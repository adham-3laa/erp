using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using erp.DTOS;
using erp.DTOS.ExpensesDTOS;
using erp.Services;

namespace erp.ViewModels
{
    public class ExpensesByAccountantViewModel : INotifyPropertyChanged
    {
        private readonly ExpenseService _expenseService;

        public ObservableCollection<ExpenseResponseDto> Expenses { get; }
            = new ObservableCollection<ExpenseResponseDto>();

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

        public ICommand LoadExpensesCommand { get; }

        public ExpensesByAccountantViewModel()
        {
            _expenseService = new ExpenseService();

            LoadExpensesCommand = new RelayCommand(async _ =>
            {
                await LoadExpenses();
            });

            // تحميل تلقائي
            _ = LoadExpenses();
        }

        private async Task LoadExpenses()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Expenses.Clear();

                var data = await _expenseService.GetExpensesByAccountant();

                foreach (var item in data)
                    Expenses.Add(item);
            }
            catch
            {
                ErrorMessage = "حدث خطأ أثناء تحميل مصروفات المحاسب";
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
