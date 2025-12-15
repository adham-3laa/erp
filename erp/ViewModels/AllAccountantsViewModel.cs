using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.Dtos;
using erp.Services;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace erp.ViewModels;

public partial class AllAccountantsViewModel : ObservableObject
{
    private readonly AccountantService _service;

    public ObservableCollection<AccountantDto> Accountants { get; } = new();

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = "";
    [ObservableProperty] private string searchText = "";

    public IAsyncRelayCommand LoadCommand { get; }
    public IRelayCommand RefreshCommand { get; }

    private List<AccountantDto> _all = new();

    public AllAccountantsViewModel()
    {
        _service = new AccountantService(ApiClient.Create());

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ApplyFilter);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private async Task LoadAsync()
    {
        try
        {
            ErrorMessage = "";
            IsBusy = true;

            var list = await _service.GetAllAccountantsAsync() ?? new List<AccountantDto>();
            _all = list;

            ApplyFilter();
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "تعذر الاتصال بالسيرفر.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var q = (SearchText ?? "").Trim().ToLowerInvariant();

        var filtered = string.IsNullOrWhiteSpace(q)
            ? _all
            : _all.Where(a =>
                   (a.Name ?? "").ToLowerInvariant().Contains(q) ||
                   (a.Email ?? "").ToLowerInvariant().Contains(q) ||
                   (a.PhoneNumber ?? "").ToLowerInvariant().Contains(q))
                .ToList();

        Accountants.Clear();
        foreach (var item in filtered)
            Accountants.Add(item);
    }
}
