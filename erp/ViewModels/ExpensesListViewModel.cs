using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class ExpensesListViewModel : INotifyPropertyChanged
    {
        private readonly ExpenseService _service;

        public ExpensesListViewModel()
        {
            _service = new ExpenseService();

            Expenses = new ObservableCollection<ExpenseResponseDto>();
            Accountants = new ObservableCollection<UserDto>(); // ✅ قائمة المحاسبين

            LoadAllCommand = new AsyncRelayCommand(LoadAll);
            LoadMyCommand = new AsyncRelayCommand(LoadMy);
            LoadByAccountantCommand = new AsyncRelayCommand(LoadByAccountant);

            AddExpenseCommand = new RelayCommand(OpenAddExpense);

            // ✅ تحميل كل المصروفات أول ما الصفحة تفتح
            _ = LoadAll();
        }

        public ObservableCollection<ExpenseResponseDto> Expenses { get; }
        public ObservableCollection<UserDto> Accountants { get; } // ✅ خاصية المحاسبين للـ ComboBox
        private UserDto _selectedAccountant;
        public UserDto SelectedAccountant
        {
            get => _selectedAccountant;
            set { _selectedAccountant = value; OnPropertyChanged(); }
        }

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

        public IAsyncRelayCommand LoadAllCommand { get; }
        public IAsyncRelayCommand LoadMyCommand { get; }
        public IAsyncRelayCommand LoadByAccountantCommand { get; }
        public ICommand AddExpenseCommand { get; }

        private async Task LoadAll()
        {
            await Load(
                () => _service.GetAllExpenses(),
                "حدث خطأ أثناء تحميل جميع المصروفات"
            );
        }

        private async Task LoadMy()
        {
            await Load(
                () => _service.GetMyExpenses(),
                "حدث خطأ أثناء تحميل مصروفاتك"
            );
        }

        private async Task LoadByAccountant()
        {
            if (SelectedAccountant == null)
            {
                ErrorMessage = "يرجى اختيار محاسب أولاً";
                return;
            }

            await Load(
                () => _service.GetExpensesByAccountant(SelectedAccountant.Id),
                "حدث خطأ أثناء تحميل مصروفات المحاسب"
            );
        }

        private async Task Load(Func<Task<List<ExpenseResponseDto>>> loader, string errorMessage)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                Expenses.Clear();

                var data = await loader();

                if (data == null || data.Count == 0)
                {
                    ErrorMessage = "لا توجد مصروفات للعرض";
                    return;
                }

                foreach (var item in data)
                    Expenses.Add(item);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unauthorized"))
                    ErrorMessage = "برجاء تسجيل الدخول مرة أخرى";
                else
                    ErrorMessage = errorMessage;
            }
            finally { IsBusy = false; }
        }

        private void OpenAddExpense()
        {
            if (Application.Current.MainWindow == null) return;

            var frame = Application.Current.MainWindow.FindName("MainFrame") as Frame;
            frame?.Navigate(new Views.Expenses.AddExpensePage());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
