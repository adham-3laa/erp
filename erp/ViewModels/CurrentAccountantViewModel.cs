using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.Dtos;
using erp.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace erp.ViewModels;

public partial class CurrentAccountantViewModel : ObservableObject
{
    private readonly AccountantService _service;

    [ObservableProperty] private AccountantDto? accountant;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = "";

    public IAsyncRelayCommand LoadCommand { get; }

    public CurrentAccountantViewModel()
    {
        _service = new AccountantService(ApiClient.Create());
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        try
        {
            ErrorMessage = "";
            IsBusy = true;

            Accountant = await _service.GetCurrentAccountantAsync();
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
}
