using System.ComponentModel.DataAnnotations;
using Core.Model;
using Core.Model.Helper;

namespace Core.Dto.User;

public class UserAiProviderRequest
{
    [Required] public UserAiProviderType Type { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public required string Key { get; set; }

    public static implicit operator UserAiProvider(UserAiProviderRequest request)
    {
        return new UserAiProvider
        {
            Type = request.Type,
            Key = request.Key
        };
    }
}