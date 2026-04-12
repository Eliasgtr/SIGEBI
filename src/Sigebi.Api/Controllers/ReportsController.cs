using Microsoft.AspNetCore.Mvc;
using Sigebi.Application.Dtos;
using Sigebi.Application.Services;

namespace Sigebi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController(ILibraryApplicationService library) : ControllerBase
{
    [HttpGet("overdue")]
    public async Task<ActionResult<IReadOnlyList<OverdueLoanReportDto>>> Overdue(CancellationToken cancellationToken)
    {
        var items = await library.GetOverdueLoansReportAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("penalties")]
    public async Task<ActionResult<IReadOnlyList<PenaltyReportDto>>> Penalties(CancellationToken cancellationToken)
    {
        var items = await library.GetActivePenaltiesReportAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }
}
