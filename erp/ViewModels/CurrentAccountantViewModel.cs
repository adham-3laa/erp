namespace EduGate.Views.Accountants
{
    public class CurrentAccountantViewModel
    {
        public AccountantDto Accountant { get; set; }

        public CurrentAccountantViewModel()
        {
            Accountant = new AccountantDto
            {
                Name = "محمد أحمد",
                Email = "m.ahmed@example.com",
                PhoneNumber = "0123456789"
            };
        }
    }

    public class AccountantDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
