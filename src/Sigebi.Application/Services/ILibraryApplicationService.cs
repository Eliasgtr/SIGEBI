using Sigebi.Application.Common;
using Sigebi.Application.Dtos;

namespace Sigebi.Application.Services;

public interface ILibraryApplicationService
{
    Task<IReadOnlyList<UserSummaryDto>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookCatalogDto>> SearchCatalogAsync(string? query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanRequestListDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserLoanDto>> GetUserLoansAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveLoanStaffDto>> GetActiveLoansForStaffAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OverdueLoanReportDto>> GetOverdueLoansReportAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PenaltyReportDto>> GetActivePenaltiesReportAsync(CancellationToken cancellationToken = default);

    Task<Result<int>> RequestLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default);
    Task<Result> ApproveLoanRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<Result> RejectLoanRequestAsync(int requestId, string reason, CancellationToken cancellationToken = default);
    Task<Result> RegisterReturnAsync(int loanId, CancellationToken cancellationToken = default);
    Task<Result> ResolvePenaltyAsync(int penaltyId, CancellationToken cancellationToken = default);

    Task<Result<int>> StaffDirectLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default);
    Task<Result<int>> RegisterBookAsync(string title, string author, string isbn, string category, string inventoryCode, CancellationToken cancellationToken = default);
    Task<Result> DeleteBookAsync(int bookId, CancellationToken cancellationToken = default);
}
