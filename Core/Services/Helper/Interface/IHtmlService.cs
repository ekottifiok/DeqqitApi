namespace Core.Services.Helper.Interface;

public interface IHtmlService
{
    Task<string?> Parse(string html);
}