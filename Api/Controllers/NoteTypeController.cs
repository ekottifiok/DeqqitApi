using System.Security.Claims;
using Core.Dto.Common;
using Core.Dto.NoteType;
using Core.Model;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NoteTypeController(INoteTypeService noteTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginationResult<NoteType>>> Get([FromQuery] PaginationRequest<int> request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<NoteType> noteTypes = await noteTypeService.Get(userId, request);

        return Ok(noteTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NoteType>> Get(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        NoteType? noteType = await noteTypeService.Get(userId, id);
        if (noteType == null) return NotFound();

        return Ok(noteType);
    }

    // TODO: Create more validation
    [HttpPost]
    public async Task<ActionResult> Create(CreateNoteTypeRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        NoteType? noteType = await noteTypeService.Create(userId, request);
        if (noteType == null) return BadRequest();

        return CreatedAtAction(nameof(Get), new { id = noteType.Id }, noteType);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateNoteTypeRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int updatedCount = await noteTypeService.Update(id, userId, request);
        if (updatedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int deletedCount = await noteTypeService.Delete(id, userId);
        if (deletedCount == 0) return NotFound();

        return NoContent();
    }
}