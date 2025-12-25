using erp.Commands;
using erp.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using erp.Services.Category;

namespace erp.ViewModels.CategoryView;

public sealed class CategoryListViewModel : BaseViewModel
{
    private readonly CategoryService _categoryService;
    private CancellationTokenSource? _cts;

    public ObservableCollection<CategoryDto> Items { get; } = new();

    private CategoryDto? _selected;
    public CategoryDto? Selected
    {
        get => _selected;
        set
        {
            if (Set(ref _selected, value))
                DeleteSelectedCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                DeleteSelectedCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand DeleteSelectedCommand { get; }

    // ✅ NEW: parameterless ctor عشان XAML يقدر ينشئ الـ VM
    public CategoryListViewModel() : this(App.Categories)
    {
    }

    public CategoryListViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => !IsBusy && Selected != null);
    }

    public async Task RefreshAsync()
    {
        CancelRunningRequest();
        _cts = new CancellationTokenSource();

        try
        {
            IsBusy = true;
            Error = null;

            var list = await _categoryService.GetAllAsync(_cts.Token);
            Items.Clear();
            foreach (var c in list)
                Items.Add(c);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (Selected == null) return;

        var confirm = MessageBox.Show(
            $"Delete '{Selected.Name}' ?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        CancelRunningRequest();
        _cts = new CancellationTokenSource();

        try
        {
            IsBusy = true;
            Error = null;

            await _categoryService.DeleteAsync(Selected.Id, _cts.Token);

            Items.Remove(Selected);
            Selected = null;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CancelRunningRequest()
    {
        try { _cts?.Cancel(); } catch { }
        try { _cts?.Dispose(); } catch { }
        _cts = null;
    }
}
