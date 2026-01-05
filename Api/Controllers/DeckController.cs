using Api.Services;
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
public class DeckController(IDeckService deckService, INoteService noteService, ICardService cardService,  ICurrentUserService currentUserService)
    : BaseController
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<DeckSummaryResponse> result = await deckService.GetSummary(userId, id);
        return ProcessResult(result);
    }

    [HttpGet("{id:int}/notes")]
    public async Task<IActionResult> GetAllNotes(int id,
        [FromQuery] PaginationRequest<int> request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<Note> notes = await noteService.Get(userId, id, request);
        return Ok(notes);
    }

    [HttpGet("{id:int}/cards")]
    public async Task<IActionResult> GetAllCards(int id,
        [FromQuery] PaginationRequest<int> request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<Card> cards = await cardService.Get(userId, id, request);
        return Ok(cards);
    }

    [HttpGet("{id:int}/cards/next")]
    public async Task<IActionResult> GetNextCard(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<CardResponse> result = await cardService.GetNextStudyCard(userId, id);
        return ProcessResult(result);
    }


    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> GetStats(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<DeckStatisticsResponse> result = await deckService.GetStatistics(userId, id);
        return ProcessResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeckRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<Deck> result = await deckService.Create(userId, request);
        // CreatedAtAction logic is a bit more complex with ProcessResult if we want to keep it exact.
        // But ProcessResult typically returns Ok(value).
        // If we strictly want CreatedAtAction, we'd need to inspect the result or use ProcessResult and accept 200 OK.
        // For standardization as per user request, using ProcessResult is safer/easier.
        return ProcessResult(result); 
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateDeckRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await deckService.Update(id, userId, request);
        return ProcessResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await deckService.Delete(id, userId);
        return ProcessResult(result);
    }
}