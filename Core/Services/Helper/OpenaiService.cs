using System.Text.Json;
using Core.Services.Helper.Interface;
using OpenAI.Chat;

// Using the official OpenAI NuGet package

namespace Core.Services.Helper;

public class OpenAiService(string apiKey) : IAiService
{
    private readonly ChatClient _client = new("gpt-5-nano", apiKey);

    public async Task<List<Dictionary<string, string>>?> GenerateFlashcard(List<string> fields, string description)
    {
        // Define the expected structure dynamically based on 'fields'
        string fieldList = string.Join(", ", fields);

        string prompt = $"Generate a JSON array of flashcards based on this description: '{description}'. " +
                        $"Each object MUST contain exactly these keys: {fieldList}.";

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "flashcard_generation",
                BinaryData.FromString($$"""
                                        {
                                            "type": "object",
                                            "properties": {
                                                "cards": {
                                                    "type": "array",
                                                    "items": {
                                                        "type": "object",
                                                        "properties": { {{string.Join(", ", fields.Select(f => $"\"{f}\": {{ \"type\": \"string\" }}"))}} },
                                                        "required": [ {{string.Join(", ", fields.Select(f => $"\"{f}\""))}} ],
                                                        "additionalProperties": false
                                                    }
                                                }
                                            },
                                            "required": ["cards"],
                                            "additionalProperties": false
                                        }
                                        """))
        };

        ChatCompletion completion = await _client.CompleteChatAsync([new UserChatMessage(prompt)], options);

        using JsonDocument doc = JsonDocument.Parse(completion.Content[0].Text);
        return doc.RootElement.GetProperty("cards").Deserialize<List<Dictionary<string, string>>>();
    }

    public async Task<int?> CheckAnswer(string question, string userAnswer, string correctAnswer)
    {
        string prompt = $"""
                         System: You are an educational evaluator. Rate the semantic closeness of the User's Answer to the Correct Answer for the given Question.

                         Scale:
                         1: Completely wrong or irrelevant.
                         2: Barely related (contains a minor keyword but missed the point).
                         3: Half-correct (got the core idea but missed major details).
                         4: Mostly correct (minor inaccuracies or slightly incomplete).
                         5: Perfect (semantically identical, even if worded differently).

                         Question: "{question}"
                         Correct Answer: "{correctAnswer}"
                         User's Answer: "{userAnswer}"

                         Respond ONLY with the number (1-5).
                         """;

        ChatCompletion completion = await _client.CompleteChatAsync(prompt);

        return int.TryParse(completion.Content[0].Text.Trim(), out int score) ? Math.Clamp(score, 1, 5) : null;
    }
}