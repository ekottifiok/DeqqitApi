using System.ComponentModel.DataAnnotations;

namespace Core.Dto.User;

// TODO: Add Validation
public class RegisterRequest
{
    public string Password { get; set; }
    public string UserName { get; set; }

    [EmailAddress] public string Email { get; set; }
    // public string ProfileImageUrl { get; set; }
}