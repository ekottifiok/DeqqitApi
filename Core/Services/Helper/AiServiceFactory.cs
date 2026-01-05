using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper.Interface;

namespace Core.Services.Helper;

public class AiServiceFactory: IAiServiceFactory
{
    public IAiService GetUserService(UserAiProvider provider)
    {
        return provider.Type switch
        {
            UserAiProviderType.ChatGpt => new OpenAiService(provider.Key),
            UserAiProviderType.Gemini => new GeminiService(provider.Key),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}