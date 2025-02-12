using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DishDashRESTAPIDatabase.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }  // In production, store a hashed password

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }  // "Admin" or "User"

        public bool IsVerified { get; set; } = false;

        // Navigation property for favorite recipes
        public ICollection<Recipe> FavoriteRecipes { get; set; } = new List<Recipe>();
    }
}
