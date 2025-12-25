using erp.Commands;
using erp.Services.Category;
using erp.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace erp.ViewModels.CategoryView;

public sealed class CategoryEditViewModel : BaseViewModel
{
    private readonly CategoryService _categoryService;
    private CancellationTokenSource? _cts;

    // ✅ Event: الصفحة هتسمعه وترجع لليست
    public event Action? RequestClose;

    // ✅ Parameterless ctor عشان XAML
    public CategoryEditViewModel() : this(App.Categories) { }

    public CategoryEditViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // ================== Properties ==================
    private string _id = "";
    public string Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            if (Set(ref _name, value))
                SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => Set(ref _description, value);
    }

    private bool _isEdit;
    public bool IsEdit
    {
        get => _isEdit;
        private set => Set(ref _isEdit, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
                SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    // ================== Commands ==================
    public AsyncRelayCommand SaveCommand { get; }

    // ================== Methods ==================
    public void LoadForCreate()
    {
        IsEdit = false;
        Id = "";
        Name = "";
        Description = null;
        Error = null;
    }

    public void LoadForEdit(CategoryDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        IsEdit = true;
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
        Error = null;
    }

    private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(Name);

    private async Task SaveAsync()
    {
        CancelRunningRequest();
        _cts = new CancellationTokenSource();

        try
        {
            IsBusy = true;
            Error = null;

            if (!IsEdit)
            {
                // Create
                var newId = await _categoryService.CreateAsync(
                    new CreateCategoryRequest
                    {
                        Name = Name.Trim(),
                        Description = Description
                    },
                    _cts.Token);

                // ✅ بعد الإضافة: ارجع لليست بدل ما تفضل في صفحة التعديل
                RequestClose?.Invoke();
                return;
            }
            else
            {
                // Update
                await _categoryService.UpdateAsync(
                    new UpdateCategoryRequest
                    {
                        Id = Id,
                        Name = Name.Trim(),
                        Description = Description
                    },
                    _cts.Token);

                // ✅ بعد التعديل: برضه ارجع لليست (لو عايز)
                RequestClose?.Invoke();
                return;
            }
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
