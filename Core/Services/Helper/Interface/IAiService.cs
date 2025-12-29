namespace Core.Services.Helper.Interface;

public interface IAiService
{
    Task<List<Dictionary<string, string>>?> GenerateFlashcard(List<string> fields, string description);

    Task<int?> CheckAnswer(string question, string userAnswer, string correctAnswer);
}