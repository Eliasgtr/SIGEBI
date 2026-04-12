using Microsoft.AspNetCore.Mvc;
using Sigebi.Application.Dtos;
using Sigebi.Application.Services;

namespace Sigebi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(ILibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var users = await library.ListUsersAsync(cancellationToken).ConfigureAwait(false);
        return Ok(users);
    }
}
