using Core.Model.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public GetMessage Get()
    {
        return new GetMessage("Welcome to Zorbtion Flashcard API");
    }

    [HttpGet("error")]
    public IActionResult GetError()
    {
        throw new Exception("What an Error Message");
    }

    [HttpGet("/error/empty")]
    public IActionResult GetErrorEmpty()
    {
        throw new Exception();
    }

    [HttpGet("/enums")]
    public AllEnumsResponse GetAllEnums()
    {
        return new AllEnumsResponse(Enum.GetNames<CardState>(),
            Enum.GetNames<DeckOptionSortOrder>(),
            Enum.GetNames<UserAiProviderType>());
    }

    public record AllEnumsResponse(
        IEnumerable<string> CardStates,
        IEnumerable<string> DeckOptionSortOrder,
        IEnumerable<string> UserAiProviderType);

    public record GetMessage(string Message);
}