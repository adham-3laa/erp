using erp.DTOS.Cheques;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels.Cheques
{
    public class EditChequeViewModel : INotifyPropertyChanged
    {
        private readonly ChequeService _service;
        private readonly int _chequeCode;

        // Properties
        private string _checkNumber = "";
        public string CheckNumber
        {
            get => _checkNumber;
            set { _checkNumber = value; OnPropertyChanged(); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        private DateTime _dueDate = DateTime.Now.AddDays(30);
        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        private string _bankName = "";
        public string BankName
        {
            get => _bankName;
            set { _bankName = value; OnPropertyChanged(); }
        }

        private bool _isIncoming = true;
        public bool IsIncoming
        {
            get => _isIncoming;
            set { _isIncoming = value; OnPropertyChanged(); }
        }

        private string _relatedName = "";
        public string RelatedName
        {
            get => _relatedName;
            set { _relatedName = value; OnPropertyChanged(); }
        }

        private string _notes = "";
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public event EventHandler OnSuccess;

        public EditChequeViewModel(ChequeDto cheque)
        {
            _service = App.Cheques;
            _chequeCode = cheque.Code;

            // Load existing data
            CheckNumber = cheque.CheckNumber;
            Amount = cheque.Amount;
            DueDate = cheque.DueDate;
            BankName = cheque.BankName;
            IsIncoming = cheque.IsIncoming;
            RelatedName = cheque.RelatedName;
            Notes = cheque.Notes;

            SaveCommand = new RelayCommand(async () => await SaveAsync());
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CheckNumber))
            {
                ErrorHandlingService.ShowWarning(ErrorHandlingService.Messages.RequiredFieldMissing + "\n(رقم الشيك)");
                return;
            }

            if (Amount <= 0)
            {
                ErrorHandlingService.ShowWarning(ErrorHandlingService.Messages.InvalidData + "\n(المبلغ يجب أن يكون أكبر من صفر)");
                return;
            }

            if (string.IsNullOrWhiteSpace(BankName))
            {
                ErrorHandlingService.ShowWarning(ErrorHandlingService.Messages.RequiredFieldMissing + "\n(اسم البنك)");
                return;
            }

            try
            {
                var request = new UpdateChequeRequest
                {
                    Code = _chequeCode,
                    CheckNumber = CheckNumber,
                    Amount = Amount,
                    DueDate = DueDate,
                    BankName = BankName,
                    IsIncoming = IsIncoming,
                    RelatedName = RelatedName,
                    Notes = Notes
                };

                var (success, errorMessage) = await _service.UpdateChequeAsync(request);
                if (success)
                {
                    ErrorHandlingService.ShowSuccess("تم تحديث الشيك بنجاح");
                    OnSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorHandlingService.ShowError(errorMessage ?? ErrorHandlingService.Messages.ChequeUpdateError);
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Updating Cheque");
                var message = ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeUpdateError);
                ErrorHandlingService.ShowError(message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
