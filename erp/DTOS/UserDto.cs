using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class UserResponseDto
    {
        [JsonPropertyName("users")]
        public List<UserDto> Users { get; set; }

        [JsonPropertyName("totalcount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("filteredcount")]
        public int FilteredCount { get; set; }
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
        public string? SalesRepId { get; set; }
        public string Phonenumber { get; set; }

        public int usernumber { get; set; }

        [JsonPropertyName("usertype")]
        public string UserType { get; set; }

        // خاصية للعرض العربي - للقراءة فقط
        private string _displayUserType;
        public string DisplayUserType
        {
            get => _displayUserType;
            set
            {
                if (_displayUserType != value)
                {
                    _displayUserType = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("isactive")]
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