using Sigebi.Application.Abstractions;
using Sigebi.Application.Common;
using Sigebi.Application.Dtos;
using Sigebi.Application.Policies;
using Sigebi.Domain.Entities;
using Sigebi.Domain.Enums;

namespace Sigebi.Application.Services;

public sealed class LibraryApplicationService(ILibraryDataAccess db) : ILibraryApplicationService
{
    public async Task<IReadOnlyList<UserSummaryDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await db.GetUsersOrderedAsync(cancellationToken).ConfigureAwait(false);
        return users
            .Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.UserType.ToString()))
            .ToList();
    }

    public async Task<IReadOnlyList<BookCatalogDto>> SearchCatalogAsync(string? query, CancellationToken cancellationToken = default)
    {
        var books = await db.SearchBooksAsync(query, cancellationToken).ConfigureAwait(false);
        return books.Select(MapCatalog).ToList();
    }

    public async Task<IReadOnlyList<LoanRequestListDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.GetPendingLoanRequestsAsync(cancellationToken).ConfigureAwait(false);
        return list.Select(r => new LoanRequestListDto(
            r.Id,
            r.UserId,
            r.User.FullName,
            r.BookCopyId,
            r.BookCopy.Book.Title,
            r.BookCopy.InventoryCode,
            r.CreatedAt)).ToList();
    }

    public async Task<IReadOnlyList<UserLoanDto>> GetUserLoansAsync(int userId, CancellationToken cancellationToken = default)
    {
        var loans = await db.GetLoansForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        return loans
            .Where(l => l.Status == LoanStatus.Active)
            .Select(l => new UserLoanDto(
                l.Id,
                l.BookCopy.Book.Title,
                l.BookCopy.InventoryCode,
                l.StartDate,
                l.DueDate,
                l.DueDate < now))
            .ToList();
    }

    public async Task<IReadOnlyList<ActiveLoanStaffDto>> GetActiveLoansForStaffAsync(CancellationToken cancellationToken = default)
    {
        var loans = await db.GetActiveLoansAsync(cancellationToken).ConfigureAwait(false);
        return loans
            .Select(l => new ActiveLoanStaffDto(
                l.Id,
                l.UserId,
                l.User.FullName,
                l.BookCopy.Book.Title,
                l.BookCopy.InventoryCode,
                l.DueDate))
            .ToList();
    }

    public async Task<IReadOnlyList<OverdueLoanReportDto>> GetOverdueLoansReportAsync(CancellationToken cancellationToken = default)
    {
        var active = await db.GetActiveLoansAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow.Date;
        return active
            .Where(l => l.DueDate < now)
            .Select(l => new OverdueLoanReportDto(
                l.Id,
                l.User.FullName,
                l.BookCopy.Book.Title,
                l.BookCopy.InventoryCode,
                l.DueDate,
                (int)(now - l.DueDate.Date).TotalDays))
            .OrderByDescending(x => x.DaysLate)
            .ToList();
    }

    public async Task<IReadOnlyList<PenaltyReportDto>> GetActivePenaltiesReportAsync(CancellationToken cancellationToken = default)
    {
        var penalties = await db.GetUnresolvedPenaltiesAsync(cancellationToken).ConfigureAwait(false);
        return penalties
            .Select(p => new PenaltyReportDto(
                p.Id,
                p.User.FullName,
                p.Reason,
                p.CreatedAt,
                p.ActiveUntil,
                p.BlocksNewLoans,
                p.IsResolved))
            .ToList();
    }

    public async Task<Result<int>> RequestLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default)
    {
        var access = await ValidateLoanAccessAsync(userId, cancellationToken).ConfigureAwait(false);
        if (access is not null)
            return Result<int>.Fail(access);

        var user = await db.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return Result<int>.Fail("Usuario no encontrado.");

        var activeCount = await db.CountActiveLoansForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        if (activeCount >= LendingPolicies.MaxConcurrentLoans(user.UserType))
            return Result<int>.Fail("El usuario alcanzó el máximo de préstamos simultáneos.");

        var copy = await db.GetCopyWithBookAsync(copyId, cancellationToken).ConfigureAwait(false);
        if (copy is null)
            return Result<int>.Fail("Ejemplar no encontrado.");
        if (copy.Status != CopyStatus.Available)
            return Result<int>.Fail("El ejemplar no está disponible.");

        var pending = await db.GetPendingLoanRequestsAsync(cancellationToken).ConfigureAwait(false);
        if (pending.Any(r => r.BookCopyId == copyId))
            return Result<int>.Fail("Ya existe una solicitud pendiente para este ejemplar.");

        var request = new LoanRequest
        {
            UserId = userId,
            BookCopyId = copyId,
            Status = LoanRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.AddLoanRequest(request);
        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.LoanRequested,
            EntityType = nameof(LoanRequest),
            EntityId = null,
            ActorUserId = userId,
            Details = $"Solicitud de préstamo para ejemplar {copy.InventoryCode} ({copy.Book.Title})"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Ok(request.Id);
    }

    public async Task<Result> ApproveLoanRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var request = await db.GetLoanRequestByIdAsync(requestId, cancellationToken).ConfigureAwait(false);
        if (request is null)
            return Result.Fail("Solicitud no encontrada.");
        if (request.Status != LoanRequestStatus.Pending)
            return Result.Fail("La solicitud ya fue procesada.");

        var access = await ValidateLoanAccessAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (access is not null)
        {
            request.Status = LoanRequestStatus.Rejected;
            request.RejectionReason = access;
            db.AddAudit(new AuditLog
            {
                Action = AuditActionType.AccessDenied,
                EntityType = nameof(LoanRequest),
                EntityId = request.Id.ToString(),
                ActorUserId = null,
                Details = access
            });
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Fail(access);
        }

        var user = await db.GetUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return Result.Fail("Usuario no encontrado.");

        var activeCount = await db.CountActiveLoansForUserAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (activeCount >= LendingPolicies.MaxConcurrentLoans(user.UserType))
            return Result.Fail("El usuario alcanzó el máximo de préstamos simultáneos.");

        var copy = await db.GetCopyWithBookAsync(request.BookCopyId, cancellationToken).ConfigureAwait(false);
        if (copy is null || copy.Status != CopyStatus.Available)
            return Result.Fail("El ejemplar ya no está disponible.");

        var start = DateTime.UtcNow;
        var due = start.AddDays(LendingPolicies.LoanDays(user.UserType));
        var loan = new Loan
        {
            UserId = request.UserId,
            BookCopyId = copy.Id,
            StartDate = start,
            DueDate = due,
            Status = LoanStatus.Active
        };

        copy.Status = CopyStatus.Loaned;
        request.Status = LoanRequestStatus.Approved;

        db.AddLoan(loan);
        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.LoanCreated,
            EntityType = nameof(Loan),
            EntityId = null,
            ActorUserId = request.UserId,
            Details = $"Préstamo aprobado. Ejemplar {copy.InventoryCode}, vence {due:O}"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result> RejectLoanRequestAsync(int requestId, string reason, CancellationToken cancellationToken = default)
    {
        var request = await db.GetLoanRequestByIdAsync(requestId, cancellationToken).ConfigureAwait(false);
        if (request is null)
            return Result.Fail("Solicitud no encontrada.");
        if (request.Status != LoanRequestStatus.Pending)
            return Result.Fail("La solicitud ya fue procesada.");

        request.Status = LoanRequestStatus.Rejected;
        request.RejectionReason = reason;

        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.LoanRequestRejected,
            EntityType = nameof(LoanRequest),
            EntityId = request.Id.ToString(),
            ActorUserId = null,
            Details = reason
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result> RegisterReturnAsync(int loanId, CancellationToken cancellationToken = default)
    {
        var loan = await db.GetLoanByIdAsync(loanId, cancellationToken).ConfigureAwait(false);
        if (loan is null)
            return Result.Fail("Préstamo no encontrado.");
        if (loan.Status != LoanStatus.Active)
            return Result.Fail("El préstamo no está activo.");

        var returnedAt = DateTime.UtcNow;
        loan.ReturnedAt = returnedAt;
        loan.Status = LoanStatus.Returned;
        loan.BookCopy.Status = CopyStatus.Available;

        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.LoanReturned,
            EntityType = nameof(Loan),
            EntityId = loan.Id.ToString(),
            ActorUserId = loan.UserId,
            Details = $"Devolución registrada el {returnedAt:O}"
        });

        if (returnedAt.Date > loan.DueDate.Date)
        {
            var penalty = new Penalty
            {
                UserId = loan.UserId,
                LoanId = loan.Id,
                Reason = "Devolución fuera de plazo",
                CreatedAt = returnedAt,
                ActiveUntil = returnedAt.Date.AddDays(LendingPolicies.PenaltyBlockDays()),
                BlocksNewLoans = true,
                IsResolved = false
            };
            db.AddPenalty(penalty);
            db.AddAudit(new AuditLog
            {
                Action = AuditActionType.PenaltyApplied,
                EntityType = nameof(Penalty),
                EntityId = null,
                ActorUserId = loan.UserId,
                Details = $"Penalización por mora hasta {penalty.ActiveUntil:O}"
            });
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result> ResolvePenaltyAsync(int penaltyId, CancellationToken cancellationToken = default)
    {
        var penalty = await db.GetPenaltyByIdAsync(penaltyId, cancellationToken).ConfigureAwait(false);
        if (penalty is null || penalty.IsResolved)
            return Result.Fail("Penalización no encontrada o ya resuelta.");

        penalty.IsResolved = true;
        penalty.BlocksNewLoans = false;

        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.InventoryChanged,
            EntityType = nameof(Penalty),
            EntityId = penalty.Id.ToString(),
            ActorUserId = null,
            Details = "Penalización marcada como resuelta por biblioteca"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result<int>> StaffDirectLoanAsync(int userId, int copyId, CancellationToken cancellationToken = default)
    {
        var access = await ValidateLoanAccessAsync(userId, cancellationToken).ConfigureAwait(false);
        if (access is not null)
            return Result<int>.Fail(access);

        var user = await db.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return Result<int>.Fail("Usuario no encontrado.");

        var activeCount = await db.CountActiveLoansForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        if (activeCount >= LendingPolicies.MaxConcurrentLoans(user.UserType))
            return Result<int>.Fail("El usuario alcanzó el máximo de préstamos simultáneos.");

        var copy = await db.GetCopyWithBookAsync(copyId, cancellationToken).ConfigureAwait(false);
        if (copy is null)
            return Result<int>.Fail("Ejemplar no encontrado.");
        if (copy.Status != CopyStatus.Available)
            return Result<int>.Fail("El ejemplar no está disponible.");

        var start = DateTime.UtcNow;
        var due = start.AddDays(LendingPolicies.LoanDays(user.UserType));
        var loan = new Loan
        {
            UserId = userId,
            BookCopyId = copy.Id,
            StartDate = start,
            DueDate = due,
            Status = LoanStatus.Active
        };
        copy.Status = CopyStatus.Loaned;

        db.AddLoan(loan);
        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.LoanCreated,
            EntityType = nameof(Loan),
            EntityId = null,
            ActorUserId = userId,
            Details = $"Préstamo directo (personal). Ejemplar {copy.InventoryCode}"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Ok(loan.Id);
    }

    public async Task<Result<int>> RegisterBookAsync(string title, string author, string isbn, string category, string inventoryCode, CancellationToken cancellationToken = default)
    {
        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Isbn = isbn.Trim(),
            Category = category.Trim()
        };
        var copy = new BookCopy
        {
            Book = book,
            InventoryCode = inventoryCode.Trim(),
            Status = CopyStatus.Available
        };
        book.Copies.Add(copy);

        db.AddBook(book);
        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.InventoryChanged,
            EntityType = nameof(Book),
            EntityId = null,
            ActorUserId = null,
            Details = $"Alta de libro '{book.Title}' con ejemplar {copy.InventoryCode}"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Ok(book.Id);
    }

    public async Task<Result> DeleteBookAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var book = await db.GetBookByIdAsync(bookId, cancellationToken).ConfigureAwait(false);
        if (book is null)
            return Result.Fail("Libro no encontrado.");

        if (await db.BookHasActiveLoansAsync(bookId, cancellationToken).ConfigureAwait(false))
            return Result.Fail("No se puede eliminar: existen préstamos activos sobre ejemplares de este libro.");

        db.RemoveBook(book);
        db.AddAudit(new AuditLog
        {
            Action = AuditActionType.InventoryChanged,
            EntityType = nameof(Book),
            EntityId = bookId.ToString(),
            ActorUserId = null,
            Details = $"Eliminación de libro '{book.Title}'"
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    private async Task<string?> ValidateLoanAccessAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await db.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return "Usuario no encontrado.";
        if (!user.IsActive)
            return "El usuario no está activo.";
        if (await db.HasOverdueActiveLoanAsync(userId, cancellationToken).ConfigureAwait(false))
            return "El usuario tiene préstamos vencidos sin regularizar.";
        if (await db.HasBlockingPenaltyAsync(userId, cancellationToken).ConfigureAwait(false))
            return "El usuario tiene una penalización activa que impide nuevos préstamos.";
        return null;
    }

    private static BookCatalogDto MapCatalog(Book book)
    {
        var copies = book.Copies.Select(c => new CopyAvailabilityDto(
            c.Id,
            c.InventoryCode,
            c.Status.ToString())).ToList();
        return new BookCatalogDto(book.Id, book.Title, book.Author, book.Isbn, book.Category, copies);
    }
}
