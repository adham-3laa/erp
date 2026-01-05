using erp.Commands;
using erp.DTOs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using erp.Services.Category;

namespace erp.ViewModels.CategoryView;

public sealed class CategoryListViewModel : BaseViewModel
{
    private readonly CategoryService _categoryService;
    private CancellationTokenSource? _cts;

    public ObservableCollection<CategoryDto> Items { get; } = new();

    private readonly ICollectionView _itemsView;
    public ICollectionView ItemsView => _itemsView;

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value))
                _itemsView.Refresh();
        }
    }

    private bool FilterItems(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (obj is not CategoryDto c)
            return false;

        return (c.Name ?? string.Empty)
            .IndexOf(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

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

    public CategoryListViewModel() : this(App.Categories) { }

    public CategoryListViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        _itemsView = CollectionViewSource.GetDefaultView(Items);
        _itemsView.Filter = FilterItems;

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

            _itemsView.Refresh();
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

            _itemsView.Refresh();
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
