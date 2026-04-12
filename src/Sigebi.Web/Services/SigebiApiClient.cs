using System.Net.Http.Json;
using System.Text.Json;
using Sigebi.Web.Models;

namespace Sigebi.Web.Services;

public sealed class SigebiApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<UserSummaryModel>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<UserSummaryModel>>("api/users", SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<IReadOnlyList<BookCatalogModel>> SearchBooksAsync(string? query, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(query)
            ? "api/books"
            : $"api/books?q={Uri.EscapeDataString(query)}";
        var list = await http.GetFromJsonAsync<List<BookCatalogModel>>(path, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<int> RequestLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/loans/request", new { userId, copyId }, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var id = await response.Content.ReadFromJsonAsync<int>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        return id;
    }

    public async Task<IReadOnlyList<UserLoanModel>> GetUserLoansAsync(int userId, CancellationToken cancellationToken = default)
    {
        var list = await http
            .GetFromJsonAsync<List<UserLoanModel>>($"api/loans/user/{userId}", SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = $"HTTP {(int)response.StatusCode}";
        try
        {
            var err = await response.Content.ReadFromJsonAsync<ApiErrorModel>(SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(err?.Error))
                message = err.Error;
        }
        catch
        {
            // ignore parse errors
        }

        throw new InvalidOperationException(message);
    }
}
