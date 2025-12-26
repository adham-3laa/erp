using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;

namespace erp.ViewModels
{
    public class ExpensesByAccountantViewModel : INotifyPropertyChanged
    {
        private readonly ExpenseService _expenseService;
        private readonly string _accountantUserId;

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

        public RelayCommand LoadExpensesCommand { get; }

        // ✅ استقبل accountantUserId عند إنشاء الـ ViewModel
        public ExpensesByAccountantViewModel(string accountantUserId)
        {
            _expenseService = new ExpenseService();
            _accountantUserId = accountantUserId;

            LoadExpensesCommand = new RelayCommand(async () => await LoadExpenses());

            // تحميل تلقائي عند فتح الصفحة
            _ = LoadExpenses();
        }

        private async Task LoadExpenses()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Expenses.Clear();

                // جلب المصروفات بناءً على الـ accountantUserId الحالي
                var data = await _expenseService.GetExpensesByAccountant(_accountantUserId);

                foreach (var item in data)
                    Expenses.Add(item);

                if (data == null || data.Count == 0)
                    ErrorMessage = "لا توجد مصروفات للعرض";
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
