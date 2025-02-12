using System.ComponentModel.DataAnnotations;

namespace DishDashRESTAPIDatabase.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
