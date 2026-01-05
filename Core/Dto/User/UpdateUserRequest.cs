using Core.Model;

namespace Core.Dto.User;

public class UpdateUserRequest
{
    public required DeckOption DeckOption { get; set; }
    public List<UserAiProviderRequest> AiProviders { get; set; } = [];
}