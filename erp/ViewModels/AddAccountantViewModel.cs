using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.Dtos;
using erp.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels;

public partial class AddAccountantViewModel : ObservableObject
{
    private readonly AccountantService _service;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string phoneNumber = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private bool isBusy;

    public IAsyncRelayCommand SaveCommand { get; }

    public AddAccountantViewModel()
    {
        _service = new AccountantService(ApiClient.Create());
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    private bool CanSave()
        => !IsBusy
           && !string.IsNullOrWhiteSpace(Name)
           && !string.IsNullOrWhiteSpace(Email)
           && !string.IsNullOrWhiteSpace(PhoneNumber)
           && !string.IsNullOrWhiteSpace(Password);

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnEmailChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPhoneNumberChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            var dto = new AccountantPostDto
            {
                Name = Name.Trim(),
                Email = Email.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                Password = Password
            };


            var ok = await _service.AddAccountantAsync(dto);
            MessageBox.Show(ok ? "تم إضافة المحاسب بنجاح ✅" : "فشل الإضافة ❌");
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("في مشكلة اتصال بالسيرفر ❌");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
