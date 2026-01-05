using System.ComponentModel.DataAnnotations;

namespace Core.Dto.User;

// TODO: Add Validation
public class RegisterRequest
{
    public required string Password { get; set; }
    public required string UserName { get; set; }

    [EmailAddress] public required string Email { get; set; }
    // public required string ProfileImageUrl { get; set; }
}