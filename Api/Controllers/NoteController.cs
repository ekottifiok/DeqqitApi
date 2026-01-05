using Api.Services;
using Core.Dto.Common;
using Core.Dto.Note;
using Core.Model;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NoteController(INoteService noteService, ICurrentUserService currentUserService): BaseController
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<Note> result = await noteService.Get(userId, id);
        return ProcessResult(result);
    }

    [HttpGet("generate")]
    public async Task<IActionResult> Generate([FromQuery] GenerateAiFlashcardRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<List<Dictionary<string, string>>> result =
            await noteService.GenerateFlashcards(userId, request.ProviderId, request.NoteTypeId, request.Description);
        return ProcessResult(result);
    }

    // TODO: Create more validation
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] List<CreateNoteRequest> request,
        [FromQuery] CreateNoteQueryRequest queryRequest)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await noteService.Create(userId, queryRequest.DeckId, queryRequest.NoteTypeId, request);
        return ProcessResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateNoteRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await noteService.Update(userId, id, request);
        return ProcessResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await noteService.Delete(id, userId);
        return ProcessResult(result);
    }
}