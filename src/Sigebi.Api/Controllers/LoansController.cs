using Microsoft.AspNetCore.Mvc;
using Sigebi.Application.Dtos;
using Sigebi.Application.Services;

namespace Sigebi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoansController(ILibraryApplicationService library) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<ActiveLoanStaffDto>>> Active(CancellationToken cancellationToken)
    {
        var items = await library.GetActiveLoansForStaffAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<LoanRequestListDto>>> Pending(CancellationToken cancellationToken)
    {
        var items = await library.GetPendingRequestsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IReadOnlyList<UserLoanDto>>> ForUser(int userId, CancellationToken cancellationToken)
    {
        var items = await library.GetUserLoansAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    public sealed record LoanRequestBody(int UserId, int CopyId);

    [HttpPost("request")]
    public async Task<ActionResult<int>> RequestLoan([FromBody] LoanRequestBody body, CancellationToken cancellationToken)
    {
        var result = await library.RequestLoanAsync(body.UserId, body.CopyId, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("{requestId:int}/approve")]
    public async Task<ActionResult> Approve(int requestId, CancellationToken cancellationToken)
    {
        var result = await library.ApproveLoanRequestAsync(requestId, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    public sealed record RejectBody(string Reason);

    [HttpPost("{requestId:int}/reject")]
    public async Task<ActionResult> Reject(int requestId, [FromBody] RejectBody body, CancellationToken cancellationToken)
    {
        var result = await library.RejectLoanRequestAsync(requestId, body.Reason, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("{loanId:int}/return")]
    public async Task<ActionResult> Return(int loanId, CancellationToken cancellationToken)
    {
        var result = await library.RegisterReturnAsync(loanId, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("direct")]
    public async Task<ActionResult<int>> DirectLoan([FromBody] LoanRequestBody body, CancellationToken cancellationToken)
    {
        var result = await library.StaffDirectLoanAsync(body.UserId, body.CopyId, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
