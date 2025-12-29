namespace Core.Services.Helper.Interface;

public interface ICssService
{
    Task<string> Parse(string html);
}