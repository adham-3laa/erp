using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;

public class AddAccountantViewModel : INotifyPropertyChanged
{
    private readonly AccountantService _service;

    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }

    public RelayCommand SaveCommand { get; set; }

    public AddAccountantViewModel()
    {
        _service = new AccountantService(new HttpClient());
        SaveCommand = new RelayCommand(async _ =>
        {
            var dto = new AccountantPostDto
            {
                Name = Name,
                Email = Email,
                Phonenumber = PhoneNumber,
                Password = Password
            };
            var success = await _service.AddAccountantAsync(dto);
            if (success) MessageBox.Show("تم إضافة المحاسب بنجاح!");
            else MessageBox.Show("حدث خطأ!");
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
