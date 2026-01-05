using Core.Dto.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult ProcessResult<T>(ResponseResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is bool or null ? NoContent() : Ok(result.Value);
        }

        return result.Error?.Code switch
        {
            ErrorCode.Unauthorized => Unauthorized(result.Error),
            ErrorCode.Forbidden => Forbid(), // Or StatusCode(403, result.Error)
            ErrorCode.NotFound => NotFound(result.Error),
            ErrorCode.Conflict or ErrorCode.AlreadyExists => Conflict(result.Error),
            ErrorCode.InvalidState  => UnprocessableEntity(result.Error),
            _ => StatusCode(500, result.Error)
        };
    }
}