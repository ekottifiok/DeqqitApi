using System.Security.Claims;
using Core.Dto.Card;
using Core.Dto.Common;
using Core.Dto.Deck;
using Core.Model;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DeckController(IDeckService deckService, INoteService noteService, ICardService cardService)
    : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeckSummaryResponse>> Get(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        DeckSummaryResponse? summary = await deckService.GetSummary(userId, id);
        if (summary == null) return NotFound();

        return Ok();
    }

    [HttpGet("{id:int}/notes")]
    public async Task<ActionResult<PaginationResult<Note>>> GetAllNotes(int id, [FromQuery]PaginationRequest<int> request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<Note> notes = await noteService.Get(userId, id, request);

        return Ok(notes);
    }

    [HttpGet("{id:int}/cards")]
    public async Task<ActionResult<PaginationResult<Card>>> GetAllCards(int id, [FromQuery]PaginationRequest<int> request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<Card> cards = await cardService.Get(userId, id, request);

        return Ok(cards);
    }

    [HttpGet("{id:int}/cards/next")]
    public async Task<ActionResult> GetNextCard(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        CardResponse? card = await cardService.GetNextStudyCard(userId, id);
        if (card == null) return NoContent();

        return Ok(card);
    }


    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<DeckStatisticsResponse>> GetStats(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        DeckStatisticsResponse? statistics = await deckService.GetStatistics(userId, id);
        if (statistics == null) return NotFound();

        return Ok(statistics);
    }

    [HttpPost]
    public async Task<ActionResult<Deck>> Create(CreateDeckRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        Deck? deck = await deckService.Create(userId, request);
        if (deck == null) return BadRequest();

        return CreatedAtAction(nameof(Get), new { id = deck.Id }, deck);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateDeckRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int updatedCount = await deckService.Update(id, userId, request);
        if (updatedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int deletedCount = await deckService.Delete(id, userId);
        if (deletedCount == 0) return NotFound();

        return NoContent();
    }
}