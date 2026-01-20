using erp.DTOS.Cheques;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels.Cheques
{
    public class AddChequeViewModel : INotifyPropertyChanged
    {
        private readonly ChequeService _service;
        private readonly UserService _userService; // For autocomplete

        // Fields
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

        private DateTime _dueDate = DateTime.Now;
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

        // ================= Autocomplete Logic =================
        private List<string> _allUsersCache = new(); // Cache all names
        public ObservableCollection<string> FilteredSuggestions { get; } = new();

        private string _relatedName = "";
        public string RelatedName
        {
            get => _relatedName;
            set 
            { 
                _relatedName = value; 
                OnPropertyChanged();
                FilterSuggestions(value);
            }
        }

        private bool _isSuggestionOpen;
        public bool IsSuggestionOpen
        {
            get => _isSuggestionOpen;
            set { _isSuggestionOpen = value; OnPropertyChanged(); }
        }

        private string _selectedSuggestion;
        public string SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    RelatedName = value; // Set text
                    IsSuggestionOpen = false; // Close list
                }
            }
        }
        // ======================================================

        private string _notes = "";
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public event EventHandler? OnSuccess;

        public AddChequeViewModel()
        {
            _service = App.Cheques;
            _userService = App.Users;
            SaveCommand = new RelayCommand(async () => await SaveAsync());
            
            // Fire and forget - load users for autocomplete
            _ = LoadUsersForAutocompleteAsync();
        }

        private async Task LoadUsersForAutocompleteAsync()
        {
            try
            {
                // Fetch a large number of users to get a good list for autocomplete
                // Adjust pageSize as needed
                var response = await _userService.GetUsersAsync(pageSize: 1000);
                if (response?.Users != null)
                {
                    // Cache names
                    _allUsersCache = response.Users
                        .Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                        .Select(u => u.Fullname)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Silently fail for autocomplete to avoid interrupting the user flow
                ErrorHandlingService.LogError(ex, "Loading users for autocomplete");
            }
        }

        private void FilterSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                IsSuggestionOpen = false;
                FilteredSuggestions.Clear();
                return;
            }

            var matches = _allUsersCache
                .Where(name => name.Contains(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name.Length) // Show shorter (more exact) matches first
                .Take(10) // Limit to 10 suggestions
                .ToList();

            FilteredSuggestions.Clear();
            foreach (var match in matches)
            {
                FilteredSuggestions.Add(match);
            }

            IsSuggestionOpen = FilteredSuggestions.Any();
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CheckNumber) || Amount <= 0 || string.IsNullOrWhiteSpace(BankName))
            {
                ErrorHandlingService.ShowWarning(ErrorHandlingService.Messages.RequiredFieldMissing + "\n(رقم الشيك، المبلغ، البنك)");
                return;
            }

            var req = new AddChequeRequest
            {
                CheckNumber = CheckNumber,
                Amount = Amount,
                DueDate = DueDate,
                BankName = BankName,
                IsIncoming = IsIncoming,
                RelatedName = RelatedName,
                Notes = Notes
            };

            try
            {
                var (success, errorMessage) = await _service.AddChequeAsync(req);
                if (success)
                {
                    ErrorHandlingService.ShowSuccess("تم حفظ الشيك بنجاح");
                    OnSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorHandlingService.ShowError(errorMessage ?? ErrorHandlingService.Messages.ChequeAddError);
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Adding Cheque");
                var message = ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeAddError);
                ErrorHandlingService.ShowError(message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
           => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

