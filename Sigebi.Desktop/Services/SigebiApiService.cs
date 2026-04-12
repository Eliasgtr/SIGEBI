using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Sigebi.Desktop.Models;

namespace Sigebi.Desktop.Services;

public sealed class SigebiApiService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public SigebiApiService(string baseUrl = "https://localhost:7081/")
    {
#if DEBUG
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
#else
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
#endif
    }

    public void Dispose() => _http.Dispose();

    public async Task<IReadOnlyList<UserSummaryModel>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var list = await _http.GetFromJsonAsync<List<UserSummaryModel>>("api/users", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<IReadOnlyList<BookCatalogModel>> SearchBooksAsync(string? query, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(query)
            ? "api/books"
            : $"api/books?q={Uri.EscapeDataString(query)}";
        var list = await _http.GetFromJsonAsync<List<BookCatalogModel>>(path, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<IReadOnlyList<LoanRequestModel>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _http.GetFromJsonAsync<List<LoanRequestModel>>("api/loans/pending", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task ApproveRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync($"api/loans/{requestId}/approve", null, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task RejectRequestAsync(int requestId, string reason, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/loans/{requestId}/reject", new { reason }, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ActiveLoanModel>> GetActiveLoansAsync(CancellationToken cancellationToken = default)
    {
        var list = await _http.GetFromJsonAsync<List<ActiveLoanModel>>("api/loans/active", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task ReturnLoanAsync(int loanId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync($"api/loans/{loanId}/return", null, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task DirectLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/loans/direct", new { userId, copyId }, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task RegisterBookAsync(string title, string author, string isbn, string category, string inventoryCode, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/books",
            new { title, author, isbn, category, inventoryCode },
            cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteBookAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync($"api/books/{bookId}", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OverdueLoanModel>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var list = await _http.GetFromJsonAsync<List<OverdueLoanModel>>("api/reports/overdue", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task<IReadOnlyList<PenaltyModel>> GetPenaltiesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _http.GetFromJsonAsync<List<PenaltyModel>>("api/reports/penalties", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    public async Task ResolvePenaltyAsync(int penaltyId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync($"api/penalties/{penaltyId}/resolve", null, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = $"HTTP {(int)response.StatusCode}";
        try
        {
            var err = await response.Content.ReadFromJsonAsync<ApiErrorModel>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(err?.Error))
                message = err.Error;
        }
        catch
        {
            // ignore
        }

        throw new InvalidOperationException(message);
    }
}
