using System.Security.Claims;
using Core.Dto.Note;
using Core.Model;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NoteController(INoteService noteService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Note>> Get(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        Note? notes = await noteService.Get(userId, id);
        if (notes == null) return NotFound();

        return Ok(notes);
    }

    [HttpGet("generate")]
    public async Task<ActionResult<List<Dictionary<string, string>>>> Generate([FromQuery] GenerateAiFlashcardRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        List<Dictionary<string, string>>? notes =
            await noteService.GenerateFlashcards(userId, request.ProviderId, request.NoteTypeId, request.Description);
        if (notes == null) return NotFound();

        return Ok(notes);
    }

    // TODO: Create more validation
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] List<CreateNoteRequest> request,
        [FromQuery] CreateNoteQueryRequest queryRequest)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        bool isSuccessful = await noteService.Create(userId, queryRequest.DeckId, queryRequest.NoteTypeId, request);
        if (!isSuccessful) return BadRequest();

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateNoteRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int updatedCount = await noteService.Update(userId, id, request);
        if (updatedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int deletedCount = await noteService.Delete(id, userId);
        if (deletedCount == 0) return NotFound();

        return NoContent();
    }
}