using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Invoices
{
    /// <summary>
    /// ViewModel for the Sales & Return Invoice (Netting Invoice) page.
    /// 
    /// Features:
    /// - Partner name autocomplete with debounced search
    /// - Optional date range filtering
    /// - Display of sales invoices, return/supply invoices
    /// - Net balance calculation with Arabic description
    /// - Print functionality
    /// </summary>
    public class SalesReturnInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;

        public SalesReturnInvoiceViewModel()
        {
            _invoiceService = new InvoiceService();

            PartnerSuggestions = new ObservableCollection<PartnerSuggestion>();
            SalesItems = new ObservableCollection<NettingInvoiceItemDto>();
            SupplyItems = new ObservableCollection<NettingInvoiceItemDto>();

            // Wire up collection change notifications
            PartnerSuggestions.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(HasPartnerSuggestions));
                OnPropertyChanged(nameof(HasNoPartnerResults));
            };

            // Commands
            SearchCommand = new RelayCommand(async () => await ExecuteSearch(), CanExecuteSearch);
            ClearCommand = new RelayCommand(ClearForm);
            PrintCommand = new RelayCommand(async () => await PrintInvoice(), () => HasData);
        }

        #region Properties - Partner Autocomplete

        private string _partnerQuery = string.Empty;
        public string PartnerQuery
        {
            get => _partnerQuery;
            set
            {
                if (_partnerQuery != value)
                {
                    _partnerQuery = value;
                    OnPropertyChanged();

                    if (!_suppressSearch)
                    {
                        // Reset partner selection flag when user starts typing again
                        _isPartnerSelected = false;

                        // Immediately set loading to true to hide "no results" message during search
                        if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
                        {
                            IsLoadingSuggestions = true;
                        }

                        // Cancel previous search logic
                        _debounceCts?.Cancel();
                        // DO NOT Dispose here to avoid race conditions with running tasks
                        // _debounceCts?.Dispose(); 
                        _debounceCts = new System.Threading.CancellationTokenSource();

                        // Trigger new search with cancellation support
                        _ = HandleAutocompleteDebounce(_partnerQuery, _debounceCts.Token);
                    }

                    // Notify AFTER resetting _isPartnerSelected
                    OnPropertyChanged(nameof(HasNoPartnerResults));
                    
                    // Re-evaluate search command availability
                    SearchCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<PartnerSuggestion> PartnerSuggestions { get; }

        public bool HasPartnerSuggestions => PartnerSuggestions.Any();
        
        // Explicit control over popup visibility
        private bool _isSuggestionsPopupOpen;
        public bool IsSuggestionsPopupOpen
        {
            get => _isSuggestionsPopupOpen;
            set { _isSuggestionsPopupOpen = value; OnPropertyChanged(); }
        }

        // Flag to track if a partner has been selected (hide "no results" message)
        private bool _isPartnerSelected;

        public bool HasNoPartnerResults =>
            !string.IsNullOrWhiteSpace(PartnerQuery) &&
            PartnerQuery.Length >= 2 &&
            !PartnerSuggestions.Any() &&
            !IsLoadingSuggestions &&
            !_isPartnerSelected;  // Hide when partner is selected

        private bool _isLoadingSuggestions;
        public bool IsLoadingSuggestions
        {
            get => _isLoadingSuggestions;
            set
            {
                _isLoadingSuggestions = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoPartnerResults));
            }
        }

        #endregion

        #region Properties - Date Filters

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

        #endregion

        #region Properties - Data

        private NettingInvoiceResponseDto _nettingData;

        public ObservableCollection<NettingInvoiceItemDto> SalesItems { get; }
        public ObservableCollection<NettingInvoiceItemDto> SupplyItems { get; }

        private string _partnerName = string.Empty;
        public string PartnerName
        {
            get => _partnerName;
            set { _partnerName = value; OnPropertyChanged(); }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private string _reportPeriod = string.Empty;
        public string ReportPeriod
        {
            get => _reportPeriod;
            set { _reportPeriod = value; OnPropertyChanged(); }
        }

        private decimal _totalSalesAmount;
        public decimal TotalSalesAmount
        {
            get => _totalSalesAmount;
            set { _totalSalesAmount = value; OnPropertyChanged(); }
        }

        private decimal _totalSupplyAmount;
        public decimal TotalSupplyAmount
        {
            get => _totalSupplyAmount;
            set { _totalSupplyAmount = value; OnPropertyChanged(); }
        }

        private decimal _netBalance;
        public decimal NetBalance
        {
            get => _netBalance;
            set { _netBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetBalanceColor)); }
        }

        private string _balanceDescription = string.Empty;
        public string BalanceDescription
        {
            get => _balanceDescription;
            set { _balanceDescription = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Color for net balance display:
        /// - Green if customer has credit (negative balance = له مستحقات)
        /// - Red if customer owes money (positive balance = عليه مستحقات)
        /// - Gray if zero
        /// </summary>
        public string NetBalanceColor =>
            NetBalance < 0 ? "#10B981" :  // Green - Customer has credit
            NetBalance > 0 ? "#EF4444" :  // Red - Customer owes
            "#6B7280";                     // Gray - Zero balance

        #endregion

        #region Properties - State

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                SearchCommand.NotifyCanExecuteChanged();
                PrintCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _hasData;
        public bool HasData
        {
            get => _hasData;
            set
            {
                _hasData = value;
                OnPropertyChanged();
                PrintCommand.NotifyCanExecuteChanged();
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        private bool _showEmptyState;
        public bool ShowEmptyState
        {
            get => _showEmptyState;
            set { _showEmptyState = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand PrintCommand { get; }

        #endregion

        #region Partner Autocomplete Logic

        private System.Threading.CancellationTokenSource _debounceCts;
        private bool _suppressSearch;

        private async Task HandleAutocompleteDebounce(string query, System.Threading.CancellationToken token)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Autocomplete started for: '{query}'");

                // Debounce delay
                await Task.Delay(300, token);

                if (token.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Autocomplete cancelled after delay");
                    return;
                }

                // UI Cleanup if query is empty
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Query too short, clearing suggestions");
                    Application.Current?.Dispatcher.Invoke(() => 
                    {
                        PartnerSuggestions.Clear();
                        IsLoadingSuggestions = false;
                    });
                    return;
                }

                // IsLoadingSuggestions is already set to true in PartnerQuery setter
                // Just ensure it's true here as well
                Application.Current?.Dispatcher.Invoke(() => IsLoadingSuggestions = true);

                try
                {
                    // Fetch data
                    System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Calling API for: '{query}'");
                    var names = await _invoiceService.GetPartnerNamesAutocompleteAsync(query);
                    System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] API returned {names?.Count ?? 0} results");

                    if (token.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Autocomplete cancelled after API call");
                        return;
                    }

                    // Process results
                    var search = query.ToLower();
                    var suggestions = new System.Collections.Generic.List<PartnerSuggestion>();

                    foreach (var name in names ?? new System.Collections.Generic.List<string>())
                    {
                        if (token.IsCancellationRequested) return;

                        var lower = name.ToLower();
                        var index = lower.IndexOf(search);

                        if (index < 0)
                        {
                            suggestions.Add(new PartnerSuggestion { FullName = name, BeforeMatch = name });
                        }
                        else
                        {
                            suggestions.Add(new PartnerSuggestion
                            {
                                FullName = name,
                                BeforeMatch = name.Substring(0, index),
                                Match = name.Substring(index, search.Length),
                                AfterMatch = name.Substring(index + search.Length)
                            });
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Processed {suggestions.Count} suggestions");

                    // Update UI
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        PartnerSuggestions.Clear();
                        foreach (var s in suggestions) PartnerSuggestions.Add(s);
                        
                        // Force popup open if we have results
                        IsSuggestionsPopupOpen = PartnerSuggestions.Any();
                        
                        System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] UI updated with {PartnerSuggestions.Count} suggestions");
                    });
                }
                finally
                {
                    Application.Current?.Dispatcher.Invoke(() => 
                    {
                        IsLoadingSuggestions = false;
                        OnPropertyChanged(nameof(HasNoPartnerResults));
                        OnPropertyChanged(nameof(HasPartnerSuggestions));
                    });
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Autocomplete task cancelled");
                Application.Current?.Dispatcher.Invoke(() => IsLoadingSuggestions = false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Autocomplete error: {ex.Message}");
                Application.Current?.Dispatcher.Invoke(() => 
                {
                    IsLoadingSuggestions = false;
                    // Optionally show error for debugging if needed, or just keep it silent to not annoy user
                    // System.Windows.MessageBox.Show($"Search Error: {ex.Message}");
                });
            }
        }

        public void SelectPartner(PartnerSuggestion item)
        {
            if (item == null) return;
            
            // Cancel and dispose pending search
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;

            // Mark that a partner was selected (hides "no results" message)
            _isPartnerSelected = true;
            IsSuggestionsPopupOpen = false; // Close popup

            _suppressSearch = true;
            try
            {
                PartnerQuery = item.FullName;
            }
            finally
            {
                _suppressSearch = false;
            }
            
            PartnerSuggestions.Clear();
            OnPropertyChanged(nameof(HasNoPartnerResults));
        }

        #endregion

        #region Search Logic

        private bool CanExecuteSearch() =>
            !IsLoading && !string.IsNullOrWhiteSpace(PartnerQuery);

        private async Task ExecuteSearch()
        {
            if (!CanExecuteSearch()) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                ShowEmptyState = false;
                HasData = false;

                // Clear previous data
                SalesItems.Clear();
                SupplyItems.Clear();

                // Fetch netting invoice data
                _nettingData = await _invoiceService.GetNettingInvoiceAsync(
                    PartnerQuery.Trim(),
                    FromDate,
                    ToDate);

                if (_nettingData == null || _nettingData.StatusCode != 200)
                {
                    ErrorMessage = _nettingData?.Message ?? "فشل في جلب البيانات";
                    ShowEmptyState = true;
                    return;
                }

                // Populate data
                PartnerName = _nettingData.PartnerName;
                PhoneNumber = _nettingData.PhoneNumber;
                ReportPeriod = $"{_nettingData.FromDate:yyyy-MM-dd} إلى {_nettingData.ToDate:yyyy-MM-dd}";

                foreach (var item in _nettingData.SalesItems ?? Enumerable.Empty<NettingInvoiceItemDto>())
                    SalesItems.Add(item);

                foreach (var item in _nettingData.SupplyItems ?? Enumerable.Empty<NettingInvoiceItemDto>())
                    SupplyItems.Add(item);

                TotalSalesAmount = _nettingData.TotalSalesAmount;
                TotalSupplyAmount = _nettingData.TotalSupplyAmount;
                NetBalance = _nettingData.NetBalance;
                BalanceDescription = _nettingData.BalanceDescription;

                HasData = SalesItems.Any() || SupplyItems.Any();
                ShowEmptyState = !HasData;

                // Close suggestions dropdown
                PartnerSuggestions.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Search error: {ex.Message}");
                ErrorMessage = $"خطأ: {ex.Message}";
                ShowEmptyState = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Clear Logic

        private void ClearForm()
        {
            // Cancel and dispose the previous cancellation token source
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;

            // Reset partner selection flag to allow autocomplete to work again
            _isPartnerSelected = false;
            IsSuggestionsPopupOpen = false;

            _suppressSearch = true;
            try
            {
                PartnerQuery = string.Empty;
            }
            finally
            {
                _suppressSearch = false;
            }
            PartnerSuggestions.Clear();

            FromDate = null;
            ToDate = null;

            SalesItems.Clear();
            SupplyItems.Clear();

            PartnerName = string.Empty;
            PhoneNumber = string.Empty;
            ReportPeriod = string.Empty;
            TotalSalesAmount = 0;
            TotalSupplyAmount = 0;
            NetBalance = 0;
            BalanceDescription = string.Empty;

            ErrorMessage = string.Empty;
            HasData = false;
            ShowEmptyState = false;

            _nettingData = null;

            // Notify UI to update "no results" visibility
            OnPropertyChanged(nameof(HasNoPartnerResults));
        }

        #endregion

        #region Print Logic

        private async Task PrintInvoice()
        {
            if (_nettingData == null || !HasData)
            {
                MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Build HTML for printing
                var html = BuildPrintHtml(_nettingData);

                // Create a temporary file for printing
                var tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"netting_invoice_{DateTime.Now:yyyyMMdd_HHmmss}.html");

                await System.IO.File.WriteAllTextAsync(tempPath, html);

                // Open in default browser for printing
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesReturnInvoiceVM] Print error: {ex.Message}");
                MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BuildPrintHtml(NettingInvoiceResponseDto data)
        {
            var salesRows = string.Join("\n", data.SalesItems?.Select((item, index) => $@"
                <tr>
                    <td>{index + 1}</td>
                    <td>{item.Date:yyyy-MM-dd}</td>
                    <td>{item.ReferenceCode}</td>
                    <td>{item.Description}</td>
                    <td class=""amount"">{item.Amount:N2}</td>
                </tr>") ?? Array.Empty<string>());

            var supplyRows = string.Join("\n", data.SupplyItems?.Select((item, index) => $@"
                <tr>
                    <td>{index + 1}</td>
                    <td>{item.Date:yyyy-MM-dd}</td>
                    <td>{item.ReferenceCode}</td>
                    <td>{item.Description}</td>
                    <td class=""amount"">{item.Amount:N2}</td>
                </tr>") ?? Array.Empty<string>());

            var balanceClass = data.NetBalance < 0 ? "credit" : data.NetBalance > 0 ? "debit" : "";

            return $@"
<!DOCTYPE html>
<html dir=""rtl"" lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <title>كشف حساب - {data.PartnerName}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Arial, sans-serif;
            padding: 20px;
            color: #000;
            background: white;
            line-height: 1.6;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #000;
        }}
        .header h1 {{
            color: #000;
            font-size: 28px;
            margin-bottom: 10px;
        }}
        .header .subtitle {{
            color: #444;
            font-size: 16px;
        }}
        .info-section {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 25px;
            padding: 15px;
            border: 1px solid #ddd;
            border-radius: 4px;
        }}
        .info-item {{
            text-align: center;
        }}
        .info-item label {{
            display: block;
            color: #555;
            font-size: 12px;
            margin-bottom: 4px;
        }}
        .info-item value {{
            display: block;
            font-weight: 600;
            color: #000;
            font-size: 15px;
        }}
        .section {{
            margin-bottom: 30px;
        }}
        .section-title {{
            background: #fff;
            color: #000;
            padding: 10px 0;
            border-bottom: 2px solid #000;
            font-size: 16px;
            font-weight: 700;
            margin-bottom: 10px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 0;
        }}
        th {{
            background: #f0f0f0;
            padding: 8px;
            text-align: right;
            font-weight: 700;
            color: #000;
            border: 1px solid #aaa;
        }}
        td {{
            padding: 8px;
            border: 1px solid #aaa;
            color: #000;
        }}
        .amount {{
            text-align: left;
            font-family: 'Consolas', monospace;
            font-weight: 700;
        }}
        .total-row {{
            background: #fff;
            font-weight: bold;
        }}
        .summary {{
            margin-top: 30px;
            padding: 20px;
            border: 2px solid #000;
            border-radius: 4px;
        }}
        .summary-grid {{
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
            text-align: center;
        }}
        .summary-item {{
            padding: 10px;
            border: 1px solid #ddd;
        }}
        .summary-item label {{
            display: block;
            color: #444;
            font-size: 13px;
            margin-bottom: 8px;
        }}
        .summary-item value {{
            display: block;
            font-size: 20px;
            font-weight: bold;
            color: #000;
        }}
        .balance-description {{
            text-align: center;
            margin-top: 20px;
            padding: 10px;
            border-top: 1px solid #ddd;
            font-size: 16px;
            font-weight: 600;
            color: #000;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            color: #666;
            font-size: 12px;
            padding-top: 10px;
            border-top: 1px solid #eee;
        }}
        @media print {{
            body {{
                padding: 0;
                color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }}
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>كشف حساب المبيعات والتوريدات</h1>
        <div class=""subtitle"">فاتورة مقاصة - Sales & Return Invoice</div>
    </div>

    <div class=""info-section"">
        <div class=""info-item"">
            <label>اسم العميل</label>
            <value>{data.PartnerName}</value>
        </div>
        <div class=""info-item"">
            <label>رقم الهاتف</label>
            <value>{(string.IsNullOrEmpty(data.PhoneNumber) ? "-" : data.PhoneNumber)}</value>
        </div>
        <div class=""info-item"">
            <label>الفترة</label>
            <value>{data.FromDate:yyyy-MM-dd} إلى {data.ToDate:yyyy-MM-dd}</value>
        </div>
        <div class=""info-item"">
            <label>تاريخ الطباعة</label>
            <value>{data.PrintDate:yyyy-MM-dd HH:mm}</value>
        </div>
    </div>

    <div class=""section"">
        <div class=""section-title"">فواتير المبيعات</div>
        <table>
            <thead>
                <tr>
                    <th style=""width: 50px"">#</th>
                    <th>التاريخ</th>
                    <th>رقم الفاتورة</th>
                    <th>الوصف</th>
                    <th style=""width: 120px"">المبلغ</th>
                </tr>
            </thead>
            <tbody>
                {(string.IsNullOrEmpty(salesRows) ? "<tr><td colspan='5' style='text-align:center; padding:20px;'>لا توجد فواتير مبيعات</td></tr>" : salesRows)}
                <tr class=""total-row"">
                    <td colspan=""4"" style=""text-align: left; font-weight: bold;"">إجمالي المبيعات</td>
                    <td class=""amount"">{data.TotalSalesAmount:N2}</td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class=""section"">
        <div class=""section-title"">فواتير التوريدات / المرتجعات</div>
        <table>
            <thead>
                <tr>
                    <th style=""width: 50px"">#</th>
                    <th>التاريخ</th>
                    <th>رقم الفاتورة</th>
                    <th>الوصف</th>
                    <th style=""width: 120px"">المبلغ</th>
                </tr>
            </thead>
            <tbody>
                {(string.IsNullOrEmpty(supplyRows) ? "<tr><td colspan='5' style='text-align:center; padding:20px;'>لا توجد فواتير توريدات</td></tr>" : supplyRows)}
                <tr class=""total-row"">
                    <td colspan=""4"" style=""text-align: left; font-weight: bold;"">إجمالي التوريدات</td>
                    <td class=""amount"">{data.TotalSupplyAmount:N2}</td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class=""summary"">
        <div class=""summary-grid"">
            <div class=""summary-item"">
                <label>إجمالي المبيعات</label>
                <value>{data.TotalSalesAmount:N2}</value>
            </div>
            <div class=""summary-item"">
                <label>إجمالي التوريدات</label>
                <value>{data.TotalSupplyAmount:N2}</value>
            </div>
            <div class=""summary-item"">
                <label>صافي الرصيد</label>
                <value>{data.NetBalance:N2}</value>
            </div>
        </div>
        <div class=""balance-description"">
            {data.BalanceDescription}
        </div>
    </div>

    <div class=""footer"">
        تم الإنشاء بواسطة نظام ERP - {DateTime.Now:yyyy-MM-dd HH:mm:ss}
    </div>

    <script>
        window.onload = function() {{
            window.print();
        }};
    </script>
</body>
</html>";
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

    /// <summary>
    /// Partner suggestion model for autocomplete display
    /// </summary>
    public class PartnerSuggestion
    {
        public string FullName { get; set; } = string.Empty;
        public string BeforeMatch { get; set; } = string.Empty;
        public string Match { get; set; } = string.Empty;
        public string AfterMatch { get; set; } = string.Empty;
    }
}
