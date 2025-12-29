using System.Text.Json;
using Core.Services.Helper.Interface;
using Google.GenAI;
using Google.GenAI.Types;

namespace Core.Services.Helper;

public class GeminiService(string apiKey) : IAiService
{
    private const string ModelName = "gemini-2.5-flash-lite"; // Updated for Gemini 2.0/2.5

    // The Client can be initialized with the key directly or via environment variables
    private readonly Client _client = new(false, apiKey);


    public Task<List<Dictionary<string, string>>?> GenerateFlashcard(List<string> fields, string description)
    {
        throw new NotImplementedException();
    }

    public async Task<int?> CheckAnswer(string question, string userAnswer, string correctAnswer)
    {
        string prompt = $"""
                         Rate the semantic closeness of the User's Answer to the Correct Answer for the given Question.
                         Scale: 1 (Wrong) to 5 (Perfect). Respond ONLY with a single number.

                         Question: "{question}"
                         Correct Answer: "{correctAnswer}"
                         User's Answer: "{userAnswer}"
                         """;

        // For evaluation, we want low temperature for high consistency
        GenerateContentConfig config = new() { Temperature = 0.1f };

        GenerateContentResponse response = await _client.Models.GenerateContentAsync(ModelName, prompt, config);
        Console.WriteLine(JsonSerializer.Serialize(response.Candidates));
        string? rawScore = response.Candidates?[0].Content?.Parts?[0].Text?.Trim() ?? null;

        return int.TryParse(rawScore, out int score) ? Math.Clamp(score, 1, 5) : null;
    }
}