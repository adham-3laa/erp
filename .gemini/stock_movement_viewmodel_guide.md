# ViewModel Updates Required for Stock Movement Report

## Properties to Add/Update

```csharp
// Add these properties to StockMovementReportViewModel.cs

public class StockMovementReportViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private bool _hasReport;
    private bool _isEmpty = true; // Default true for initial state
    private string _productName;
    private StockMovementReportDto _report;

    // Existing or Updated Properties
    public string ProductName
    {
        get => _productName;
        set
        {
            _productName = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading)); // For button enable/disable
        }
    }

    public bool IsNotLoading => !IsLoading;

    public bool HasReport
    {
        get => _hasReport;
        set
        {
            _hasReport = value;
            OnPropertyChanged();
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set
        {
            _isEmpty = value;
            OnPropertyChanged();
        }
    }

    public StockMovementReportDto Report
    {
        get => _report;
        set
        {
            _report = value;
            OnPropertyChanged();
        }
    }

    // Commands
    public ICommand LoadReportCommand { get; private set; }
    public ICommand ClearSearchCommand { get; private set; }

    // Constructor
    public StockMovementReportViewModel()
    {
        LoadReportCommand = new RelayCommand(async () => await LoadReport());
        ClearSearchCommand = new RelayCommand(ClearSearch);
    }

    // Methods
    private async Task LoadReport()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            // Show validation message (optional)
            return;
        }

        try
        {
            IsLoading = true;
            IsEmpty = false; // Hide empty state while loading
            HasReport = false; // Hide previous report

            // Call API
            var result = await _reportService.GetStockMovementReport(ProductName);

            if (result != null && result.StatusCode == 200)
            {
                Report = result;
                HasReport = true;
                IsEmpty = false;
            }
            else
            {
                // No data found
                HasReport = false;
                IsEmpty = true;
            }
        }
        catch (Exception ex)
        {
            // Handle error
            HasReport = false;
            IsEmpty = true;
            // Show error message to user
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearSearch()
    {
        ProductName = string.Empty;
        HasReport = false;
        IsEmpty = true;
        Report = null;
    }
}
```

## States Logic

### Initial State (Page Load)
```csharp
IsEmpty = true       // Show empty state
HasReport = false    // Hide report sections
IsLoading = false    // No loading overlay
```

### Loading State (User clicks Download)
```csharp
IsEmpty = false      // Hide empty state
HasReport = false    // Hide previous report
IsLoading = true     // Show loading overlay + disable button
```

### Success State (Data Found)
```csharp
IsEmpty = false      // Hide empty state
HasReport = true     // Show all report sections
IsLoading = false    // Hide loading overlay
```

### No Data State (Product Not Found)
```csharp
IsEmpty = true       // Show "لا توجد حركات مسجلة" message
HasReport = false    // Hide report sections
IsLoading = false    // Hide loading overlay
```

### After Clear
```csharp
IsEmpty = true       // Back to initial empty state
HasReport = false    // Hide report
IsLoading = false    // No loading
ProductName = ""     // Clear input
```

## Keyboard Support (Enter Key)

The `UpdateSourceTrigger=PropertyChanged` on the TextBox allows the binding to update immediately. To support Enter key to trigger search, add this to the code-behind:

```csharp
// In StockMovementReportPage.xaml.cs

public partial class StockMovementReportPage : Page
{
    public StockMovementReportPage()
    {
        InitializeComponent();
    }

    private void ProductNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var viewModel = DataContext as StockMovementReportViewModel;
            if (viewModel?.LoadReportCommand?.CanExecute(null) == true)
            {
                viewModel.LoadReportCommand.Execute(null);
            }
        }
    }
}
```

And update the TextBox in XAML:
```xml
<TextBox x:Name="ProductNameTextBox"
         Text="{Binding ProductName, UpdateSourceTrigger=PropertyChanged}"
         KeyDown="ProductNameTextBox_KeyDown"
         .../>
```

## Checklist

- [ ] Add `IsLoading` property with `IsNotLoading` computed property
- [ ] Add `HasReport` boolean property
- [ ] Add `IsEmpty` boolean property (default `true`)
- [ ] Implement `ClearSearchCommand`
- [ ] Update `LoadReportCommand` to set states correctly
- [ ] Add try-catch error handling
- [ ] Add keyboard support for Enter key
- [ ] Test all state transitions
- [ ] Verify empty state shows on initial load
- [ ] Verify loading overlay appears during fetch
- [ ] Verify buttons disable during loading
- [ ] Verify clear button resets everything

## Notes

- The `IsNotLoading` property is crucial for disabling the Download button during loading
- Make sure `IsEmpty` defaults to `true` so users see the helpful empty state initially
- The state management ensures users never see both empty state and data at the same time
- Clear button provides quick way to start a new search
