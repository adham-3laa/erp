using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace erp.ViewModels.Invoices
{
    public class InvoicesListViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesListViewModel()
        {
            _invoiceService = new InvoiceService();
            Invoices = new ObservableCollection<InvoiceResponseDto>();

            LoadInvoicesCommand = new RelayCommand(async () => await LoadInvoices());
            NextPageCommand = new RelayCommand(async () => await NextPage(), () => HasNextPage);
            PreviousPageCommand = new RelayCommand(async () => await PreviousPage(), () => Page > 1);
        }

        // ================= Data =================

        public ObservableCollection<InvoiceResponseDto> Invoices { get; }

        // ================= Filters =================

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
            }
        }

        private string _invoiceType;
        public string InvoiceType
        {
            get => _invoiceType;
            set
            {
                _invoiceType = value;
                OnPropertyChanged();
            }
        }

        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                _query = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        // ================= Paging =================

        private int _page = 1;
        public int Page
        {
            get => _page;
            set
            {
                _page = value;
                OnPropertyChanged();
                RaisePagingCommands();
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }

        private bool _hasNextPage = true;
        public bool HasNextPage
        {
            get => _hasNextPage;
            set
            {
                _hasNextPage = value;
                OnPropertyChanged();
                RaisePagingCommands();
            }
        }

        // ================= UI State =================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // ================= Commands =================

        public RelayCommand LoadInvoicesCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }


        // ================= Logic =================

        private async Task LoadInvoices()
        {
            try
            {
                IsLoading = true;
                Invoices.Clear();

                var data = await _invoiceService.GetInvoices(
                    Search,
                    InvoiceType,
                    Query,
                    FromDate,
                    ToDate,
                    Page,
                    PageSize);

                foreach (var invoice in data)
                    Invoices.Add(invoice);

                // لو عدد النتائج أقل من PageSize → مفيش صفحة بعدها
                HasNextPage = data.Count == PageSize;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NextPage()
        {
            Page++;
            await LoadInvoices();
        }

        private async Task PreviousPage()
        {
            if (Page > 1)
            {
                Page--;
                await LoadInvoices();
            }
        }

        private void RaisePagingCommands()
        {
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }

        // ================= INotify =================

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
