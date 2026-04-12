using Microsoft.EntityFrameworkCore;
using Sigebi.Application.Abstractions;
using Sigebi.Domain.Entities;
using Sigebi.Domain.Enums;

namespace Sigebi.Infrastructure.Persistence;

public sealed class LibraryDataAccess(SigebiDbContext db) : ILibraryDataAccess
{
    public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<User>> GetUsersOrderedAsync(CancellationToken cancellationToken = default) =>
        await db.Users
            .OrderBy(u => u.FullName)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<BookCopy?> GetCopyWithBookAsync(int copyId, CancellationToken cancellationToken = default) =>
        await db.BookCopies
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == copyId, cancellationToken)
            .ConfigureAwait(false);

    public Task<int> CountActiveLoansForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Loans.CountAsync(l => l.UserId == userId && l.Status == LoanStatus.Active, cancellationToken);

    public Task<bool> HasOverdueActiveLoanAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return db.Loans.AnyAsync(
            l => l.UserId == userId && l.Status == LoanStatus.Active && l.DueDate < now,
            cancellationToken);
    }

    public Task<bool> HasBlockingPenaltyAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return db.Penalties.AnyAsync(
            p => p.UserId == userId
                 && !p.IsResolved
                 && p.BlocksNewLoans
                 && (p.ActiveUntil == null || p.ActiveUntil >= now),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> SearchBooksAsync(string? query, CancellationToken cancellationToken = default)
    {
        var q = db.Books
            .Include(b => b.Copies)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(b =>
                b.Title.Contains(term)
                || b.Author.Contains(term)
                || b.Isbn.Contains(term)
                || b.Category.Contains(term));
        }

        return await q
            .OrderBy(b => b.Title)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<LoanRequest>> GetPendingLoanRequestsAsync(CancellationToken cancellationToken = default) =>
        await db.LoanRequests
            .Include(r => r.User)
            .Include(r => r.BookCopy)
            .ThenInclude(c => c.Book)
            .Where(r => r.Status == LoanRequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<LoanRequest?> GetLoanRequestByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.LoanRequests
            .Include(r => r.User)
            .Include(r => r.BookCopy)
            .ThenInclude(c => c.Book)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public async Task<Loan?> GetLoanByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Loans
            .Include(l => l.User)
            .Include(l => l.BookCopy)
            .ThenInclude(c => c.Book)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<bool> BookHasActiveLoansAsync(int bookId, CancellationToken cancellationToken = default) =>
        db.Loans.AnyAsync(
            l => l.Status == LoanStatus.Active && l.BookCopy.BookId == bookId,
            cancellationToken);

    public async Task<IReadOnlyList<Loan>> GetActiveLoansAsync(CancellationToken cancellationToken = default) =>
        await db.Loans
            .Include(l => l.User)
            .Include(l => l.BookCopy)
            .ThenInclude(c => c.Book)
            .Where(l => l.Status == LoanStatus.Active)
            .OrderBy(l => l.DueDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Loan>> GetLoansForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.Loans
            .Include(l => l.BookCopy)
            .ThenInclude(c => c.Book)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Penalty>> GetUnresolvedPenaltiesAsync(CancellationToken cancellationToken = default) =>
        await db.Penalties
            .Include(p => p.User)
            .Where(p => !p.IsResolved)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<Penalty?> GetPenaltyByIdAsync(int penaltyId, CancellationToken cancellationToken = default) =>
        db.Penalties.FirstOrDefaultAsync(p => p.Id == penaltyId, cancellationToken);

    public void AddLoanRequest(LoanRequest request) => db.LoanRequests.Add(request);

    public void AddLoan(Loan loan) => db.Loans.Add(loan);

    public void AddBook(Book book) => db.Books.Add(book);

    public void RemoveBook(Book book) => db.Books.Remove(book);

    public void AddAudit(AuditLog log) => db.AuditLogs.Add(log);

    public void AddPenalty(Penalty penalty) => db.Penalties.Add(penalty);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
