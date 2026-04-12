using Microsoft.AspNetCore.Mvc;
using Sigebi.Application.Services;

namespace Sigebi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PenaltiesController(ILibraryApplicationService library) : ControllerBase
{
    [HttpPost("{penaltyId:int}/resolve")]
    public async Task<ActionResult> Resolve(int penaltyId, CancellationToken cancellationToken)
    {
        var result = await library.ResolvePenaltyAsync(penaltyId, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
