using System.Collections.ObjectModel;

namespace EduGate.Views.Accountants
{
    public class AllAccountantsViewModel
    {
        public ObservableCollection<AccountantDto> Accountants { get; set; }

        public AllAccountantsViewModel()
        {
            Accountants = new ObservableCollection<AccountantDto>
            {
                new AccountantDto { Name = "محمد", Email = "m@example.com", PhoneNumber = "0123456789" },
                new AccountantDto { Name = "أحمد", Email = "a@example.com", PhoneNumber = "0987654321" }
            };
        }
    }

   
}
