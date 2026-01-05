using Api.Services;
using Core.Dto.Card;
using Core.Dto.Common;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]/{id:int}")]
[ApiController]
public class CardController(ICardService cardService, ICurrentUserService currentUserService): BaseController
{
    [HttpPut("")]
    public async Task<IActionResult> Update(int id, UpdateCardStateRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await cardService.UpdateCardState(userId, id, request);
        return ProcessResult(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit(int id, CardSubmitRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<CardResponse> result = await cardService.SubmitCardReview(userId, id, request);
        return ProcessResult(result);
    }
}