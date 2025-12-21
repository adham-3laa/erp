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
    public class ExpensesListViewModel : INotifyPropertyChanged
    {
        private readonly ExpenseService _expenseService;

        public ObservableCollection<ExpenseResponseDto> Expenses { get; }
            = new ObservableCollection<ExpenseResponseDto>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LoadMyExpensesCommand { get; }
        public ICommand LoadExpensesByAccountantCommand { get; }
        public ICommand AddExpenseCommand { get; }

        public ExpensesListViewModel()
        {
            _expenseService = new ExpenseService();

            LoadMyExpensesCommand =
                new RelayCommand(async _ => await LoadMyExpenses());

            LoadExpensesByAccountantCommand =
                new RelayCommand(async _ => await LoadExpensesByAccountant());

            AddExpenseCommand = new RelayCommand(_ =>
            {
                var frame = System.Windows.Application.Current.MainWindow
                    .FindName("MainFrame") as System.Windows.Controls.Frame;

                frame?.Navigate(new erp.Views.Expenses.AddExpensePage());
            });

            _ = LoadMyExpenses();
        }

        // ================= Logic =================
        private async Task LoadMyExpenses()
        {
            await Load(async () => await _expenseService.GetMyExpenses(),
                 "حدث خطأ أثناء تحميل مصروفاتي");
        }

        private async Task LoadExpensesByAccountant()
        {
            await Load(async () => await _expenseService.GetExpensesByAccountant(),
                 "حدث خطأ أثناء تحميل مصروفات المحاسب");
        }

        private async Task Load(
            Func<Task<List<ExpenseResponseDto>>> loader,
            string errorMsg)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Expenses.Clear();

                var data = await loader();

                foreach (var item in data)
                    Expenses.Add(item);
            }
            catch
            {
                ErrorMessage = errorMsg;
            }
            finally
            {
                IsBusy = false;
            }
        }
        // ================= INotifyPropertyChanged =================
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
