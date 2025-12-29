using System.ComponentModel.DataAnnotations;
using Core.Model;
using Core.Model.Helper;

namespace Core.Dto.User;

public class UserAiProviderRequest
{
    [Required] public UserAiProviderType Type { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Key { get; set; }
    
    public static implicit operator UserAiProvider(UserAiProviderRequest request) => new()
    {
        Type = request.Type,
        Key = request.Key,
    };

}