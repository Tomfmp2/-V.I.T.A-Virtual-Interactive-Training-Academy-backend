using Microsoft.AspNetCore.Mvc;

namespace Vita.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiError(int statusCode, string message)
        => StatusCode(statusCode, new { error = message, statusCode });
}