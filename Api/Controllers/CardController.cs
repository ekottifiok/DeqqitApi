using System.Security.Claims;
using Core.Dto.Card;
using Core.Services;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]/{id:int}")]
[ApiController]
public class CardController(ICardService cardService) : ControllerBase
{
    [HttpPut("")]
    public async Task<ActionResult> Update(int id, UpdateCardStateRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int updatedCount = await cardService.UpdateCardState(userId, id, request);
        if (updatedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpPost("submit")]
    public async Task<ActionResult> Submit(int id, CardSubmitRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        bool isSuccessful = await cardService.SubmitCardReview(userId, id, request);
        if (!isSuccessful) return BadRequest();

        return NoContent();
    }
}