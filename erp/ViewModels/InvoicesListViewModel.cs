using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels.Invoices
{
    public class InvoicesListViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesListViewModel()
        {
            _invoiceService = new InvoiceService();
            Invoices = new ObservableCollection<InvoiceResponseDto>();

            InvoiceType = "الكل";
            IsLastInvoice = "الكل";

            LoadInvoicesCommand = new RelayCommand(async () => await LoadInvoices(true));
            NextPageCommand = new RelayCommand(async () => await NextPage(), () => HasNextPage);
            PreviousPageCommand = new RelayCommand(async () => await PreviousPage(), () => Page > 1);
        }


        // ================= Data =================

        public ObservableCollection<InvoiceResponseDto> Invoices { get; }

        // ================= Filters =================

        // 🔍 بحث عام
        private string _search;
        public string Search
        {
            get => _search;
            set { _search = value; OnPropertyChanged(); }
        }

        // 🧾 نوع الفاتورة (القيمة العربية من الـ UI)
        private string _invoiceType;
        public string InvoiceType
        {
            get => _invoiceType;
            set { _invoiceType = value; OnPropertyChanged(); }
        }

        // ✅ تحويل القيمة العربية للقيمة اللي Swagger مستنيها
        private string InvoiceTypeApiValue =>
            string.IsNullOrWhiteSpace(InvoiceType) || InvoiceType == "الكل"
                ? null
                : InvoiceType switch
                {
                    "فواتير العملاء" => "CustomerInvoice",
                    "فواتير العمولات" => "CommissionInvoice",
                    "فواتير الموردين" => "SupplierInvoice",
                    "فواتير المرتجعات" => "ReturnInvoice",
                    _ => null
                };


        // 🆔 رقم الطلب
        private string _orderId;
        public string OrderId
        {
            get => _orderId;
            set { _orderId = value; OnPropertyChanged(); }
        }

        // 👤 رقم أو اسم المستلم
        private string _recipientQuery;
        public string RecipientQuery
        {
            get => _recipientQuery;
            set { _recipientQuery = value; OnPropertyChanged(); }
        }

        // 📅 من تاريخ
        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set { _fromDate = value; OnPropertyChanged(); }
        }

        // 📅 إلى تاريخ
        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set { _toDate = value; OnPropertyChanged(); }
        }

        // 🧾 آخر فاتورة (نعم / لا)
        private string _isLastInvoice;
        public string IsLastInvoice
        {
            get => _isLastInvoice;
            set { _isLastInvoice = value; OnPropertyChanged(); }
        }

        // تحويل القيمة العربية إلى bool? علشان الـ API
        private bool? IsLastInvoiceBool =>
            IsLastInvoice == "نعم" ? true :
            IsLastInvoice == "لا" ? false :
            null;


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

        public int PageSize { get; } = 10;

        private bool _hasNextPage;
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
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ================= Commands =================

        public RelayCommand LoadInvoicesCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }

        // ================= Logic =================
        private async Task LoadInvoices(bool resetPage)
        {
            try
            {
                if (resetPage)
                    Page = 1;

                Invoices.Clear();

                var response = await _invoiceService.GetInvoices(
                    search: Search,
                    invoiceType: InvoiceTypeApiValue,
                    query: RecipientQuery,
                    orderId: OrderId,
                    lastInvoice: IsLastInvoiceBool,
                    fromDate: FromDate,
                    toDate: ToDate,
                    page: Page,
                    pageSize: PageSize
                );

                if (response?.Items == null)
                    return;

                foreach (var invoice in response.Items)
                    Invoices.Add(invoice);

                HasNextPage = (Page * PageSize) < response.TotalItems;
            }
            catch
            {
                HasNextPage = false;
            }
        }

        private async Task NextPage()
        {
            if (!HasNextPage) return;

            Page++;
            await LoadInvoices(false);
        }

        private async Task PreviousPage()
        {
            if (Page <= 1) return;

            Page--;
            await LoadInvoices(false);
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
