using System;
using System.Text.Json.Serialization;

namespace erp.Dtos
{
    public class CurrentUserDto
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        [JsonPropertyName("usertype")]
        public string UserType { get; set; }
        [JsonPropertyName("isactive")]
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
        [JsonPropertyName("dateofcreation")]
        public DateTime DateOfCreation { get; set; }
        public int FarmsCount { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}