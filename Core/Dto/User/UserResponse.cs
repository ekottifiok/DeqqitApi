using Core.Model;

namespace Core.Dto.User;

public class UserResponse
{
    public string UserId { get; set; }
    public string? UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<DateOnly> UserStreaks { get; set; } = [];
    public DeckOption DeckOption { get; set; }
    public List<UserAiProvider> AiProviders { get; set; } = [];

    // The Implicit Converter
    public static implicit operator UserResponse(Model.User user)
    {

        return new UserResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            ProfileImageUrl = user.ProfileImageUrl,
            UserStreaks = user.UserStreaks,
            DeckOption = user.DeckOption,
            AiProviders = user.AiProviders
        };
    }
}