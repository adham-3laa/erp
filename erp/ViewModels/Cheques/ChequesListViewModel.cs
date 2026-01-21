using erp.DTOS.Cheques;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Linq; // For filtering if doing client side, though service does server side

namespace erp.ViewModels.Cheques
{
    public class ChequesListViewModel : INotifyPropertyChanged
    {
        private readonly ChequeService _service;

        public ObservableCollection<ChequeDto> Cheques { get; } = new();

        // Filters
        private string _searchName = "";
        public string SearchName
        {
            get => _searchName;
            set { _searchName = value; OnPropertyChanged(); LoadChequesCommand.Execute(null); }
        }
        
        private string _selectedDirection = "";
        public string SelectedDirection
        {
            get => _selectedDirection;
            set { _selectedDirection = value; OnPropertyChanged(); LoadChequesCommand.Execute(null); }
        }

        private string _selectedStatus = "";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); LoadChequesCommand.Execute(null); }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); LoadChequesCommand.Execute(null); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); LoadChequesCommand.Execute(null); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoadChequesCommand { get; }
        public ICommand UpdateStatusCommand { get; }

        public ChequesListViewModel()
        {
            _service = App.Cheques;
            LoadChequesCommand = new RelayCommand(async () => await LoadChequesAsync());
            UpdateStatusCommand = new RelayCommand(async (param) => 
            {
                if (param is (ChequeDto chq, string newStatus))
                {
                    await UpdateStatusAsync(chq.Code, newStatus);
                }
            });

            // Initial load
            LoadChequesCommand.Execute(null);
        }

        public async Task LoadChequesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                var list = await _service.GetAllChequesAsync(SearchName, SelectedDirection, SelectedStatus);
                
                // Client side date filtering
                if (StartDate.HasValue) list = list.Where(x => x.DueDate >= StartDate.Value).ToList();
                if (EndDate.HasValue) list = list.Where(x => x.DueDate <= EndDate.Value).ToList();

                Cheques.Clear();
                foreach (var c in list) Cheques.Add(c);
            }
            catch (Exception ex)
            {
                // ServiceException is already user-friendly, others are translated
                ErrorHandlingService.LogError(ex, "Loading Cheques");
                var message = ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeLoadError);
                ErrorHandlingService.ShowError(message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateStatusAsync(int code, string newStatus)
        {
            IsBusy = true;
            try
            {
                var (success, errorMessage) = await _service.UpdateChequeStatusAsync(code, newStatus);
                
                if (success)
                {
                    ErrorHandlingService.ShowSuccess($"تم تحديث حالة الشيك إلى '{newStatus}' بنجاح.");
                    await LoadChequesAsync();
                }
                else
                {
                    ErrorHandlingService.ShowError(errorMessage ?? ErrorHandlingService.Messages.ChequeStatusUpdateError);
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Updating Cheque Status");
                var message = ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeStatusUpdateError);
                ErrorHandlingService.ShowError(message);
            }
            finally
            {
                IsBusy = false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
