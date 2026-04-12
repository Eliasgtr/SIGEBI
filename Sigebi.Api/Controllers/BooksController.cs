using Microsoft.AspNetCore.Mvc;
using Sigebi.Application.Dtos;
using Sigebi.Application.Services;

namespace Sigebi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController(ILibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookCatalogDto>>> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var books = await library.SearchCatalogAsync(q, cancellationToken).ConfigureAwait(false);
        return Ok(books);
    }

    public sealed record RegisterBookRequest(string Title, string Author, string Isbn, string Category, string InventoryCode);

    [HttpPost]
    public async Task<ActionResult<int>> Register([FromBody] RegisterBookRequest body, CancellationToken cancellationToken)
    {
        var result = await library.RegisterBookAsync(body.Title, body.Author, body.Isbn, body.Category, body.InventoryCode, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await library.DeleteBookAsync(id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
