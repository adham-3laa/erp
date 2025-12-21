using erp.Commands;
using erp.DTOs;
using erp.ViewModels;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace erp.ViewModels.CategoryView;

public sealed class CategoryEditViewModel : BaseViewModel
{
    private CancellationTokenSource? _cts;

    private string _id = "";
    public string Id { get => _id; set => Set(ref _id, value); }

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
    public string? Description { get => _description; set => Set(ref _description, value); }

    private bool _isEdit;
    public bool IsEdit { get => _isEdit; private set => Set(ref _isEdit, value); }

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
    public string? Error { get => _error; private set => Set(ref _error, value); }

    public AsyncRelayCommand SaveCommand { get; }

    public CategoryEditViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

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
        IsEdit = true;
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
        Error = null;
    }

    private bool CanSave()
        => !IsBusy && !string.IsNullOrWhiteSpace(Name);

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
                var created = await App.Categories.CreateAsync(
                    new CreateCategoryRequest { Name = Name.Trim(), Description = Description },
                    _cts.Token);

                LoadForEdit(created);
            }
            else
            {
                var updated = await App.Categories.UpdateAsync(
                    new UpdateCategoryRequest { Id = Id, Name = Name.Trim(), Description = Description },
                    _cts.Token);

                LoadForEdit(updated);
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
