using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using erp.DTOS.ExpensesDTOS;
using erp.Services;

namespace erp.ViewModels
{
    public class AddExpenseViewModel : INotifyPropertyChanged
    {
        private decimal _amount;
        private string _description;
        private string _errorMessage;
        private bool _isBusy;
        private bool _isSuccess;

        private readonly ExpenseService _expenseService;

        public AddExpenseViewModel()
        {
            // Initialize service with token
            _expenseService = new ExpenseService();


            SaveCommand = new RelayCommand(async _ => await SaveExpense(), _ => !IsBusy);
        }

        #region Properties

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }

        #endregion

        #region Methods

        private async Task SaveExpense()
        {
            ErrorMessage = string.Empty;
            IsSuccess = false;

            if (Amount <= 0)
            {
                ErrorMessage = "Amount must be greater than zero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Description is required.";
                return;
            }

            IsBusy = true;

            try
            {
                var dto = new ExpenseCreateDto
                {
                    Amount = this.Amount,
                    Description = this.Description
                };

                var response = await _expenseService.AddExpense(dto);

                // Success
                IsSuccess = true;
                Amount = 0;
                Description = string.Empty;

                // Optionally, you can store/display response.AccountantName / response.CreatedAt
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
