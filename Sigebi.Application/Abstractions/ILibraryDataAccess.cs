using Sigebi.Domain.Entities;

namespace Sigebi.Application.Abstractions;

public interface ILibraryDataAccess
{
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersOrderedAsync(CancellationToken cancellationToken = default);
    Task<BookCopy?> GetCopyWithBookAsync(int copyId, CancellationToken cancellationToken = default);
    Task<int> CountActiveLoansForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> HasOverdueActiveLoanAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> HasBlockingPenaltyAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> SearchBooksAsync(string? query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanRequest>> GetPendingLoanRequestsAsync(CancellationToken cancellationToken = default);
    Task<LoanRequest?> GetLoanRequestByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Loan?> GetLoanByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> BookHasActiveLoansAsync(int bookId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> GetActiveLoansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> GetLoansForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Penalty>> GetUnresolvedPenaltiesAsync(CancellationToken cancellationToken = default);
    Task<Penalty?> GetPenaltyByIdAsync(int penaltyId, CancellationToken cancellationToken = default);

    void AddLoanRequest(LoanRequest request);
    void AddLoan(Loan loan);
    void AddBook(Book book);
    void RemoveBook(Book book);
    void AddAudit(AuditLog log);
    void AddPenalty(Penalty penalty);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
