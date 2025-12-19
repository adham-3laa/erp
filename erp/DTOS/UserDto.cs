//using System;

//namespace erp.DTOS
//{
//    public class UserDto
//    {
//        public string Id { get; set; }
//        public string Fullname { get; set; }
//        public string Username { get; set; }
//        public string Email { get; set; }
//        public string Phonenumber { get; set; }
//        public string UserType { get; set; }
//        public bool IsActive { get; set; }
//        public string ImagePath { get; set; }
//        public DateTime DateOfCreation { get; set; }
//        public int FarmsCount { get; set; }
//    }

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;


namespace erp.DTOS
{
    public class UserResponseDto
    {
        public List<UserDto> Users { get; set; }
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
    }

    public class UserPostDto
    {
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public string Password { get; set; }
        [JsonPropertyName("usertype")]
        public string UserType { get; set; }
    }

    public class UserStatusDto
    {

        [JsonPropertyName("isactive")]
        public bool IsActive { get; set; }
    }



    public class UserDto : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        [JsonPropertyName("usertype")]
        public string UserType { get; set; }
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath { get; set; }

        [JsonPropertyName("dateofcreation")]
        public DateTime DateOfCreation { get; set; }
        public int FarmsCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }


}