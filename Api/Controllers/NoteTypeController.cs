using Api.Services;
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
public class NoteTypeController(INoteTypeService noteTypeService, ICurrentUserService currentUserService): BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PaginationRequest<int> request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        PaginationResult<NoteType> noteTypes = await noteTypeService.Get(userId, request);
        return Ok(noteTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<NoteType> result = await noteTypeService.Get(userId, id);
        return ProcessResult(result);
    }

    // TODO: Create more validation
    [HttpPost]
    public async Task<IActionResult> Create(CreateNoteTypeRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<NoteType> result = await noteTypeService.Create(userId, request);
        return ProcessResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateNoteTypeRequest request)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await noteTypeService.Update(id, userId, request);
        return ProcessResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await noteTypeService.Delete(id, userId);
        return ProcessResult(result);
    }
}