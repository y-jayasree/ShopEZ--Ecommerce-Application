using System.ComponentModel.DataAnnotations;

namespace ShopEZ_Shared_Lib.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = "";

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = "";
        public UserDTO User { get; set; } = new();
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}