namespace Sigebi.Application.Dtos;

public sealed record BookCatalogDto(
    int BookId,
    string Title,
    string Author,
    string Isbn,
    string Category,
    IReadOnlyList<CopyAvailabilityDto> Copies);

public sealed record CopyAvailabilityDto(int CopyId, string InventoryCode, string Status);

public sealed record LoanRequestListDto(
    int Id,
    int UserId,
    string UserName,
    int CopyId,
    string BookTitle,
    string InventoryCode,
    DateTime CreatedAt);

public sealed record UserLoanDto(
    int LoanId,
    string BookTitle,
    string InventoryCode,
    DateTime StartDate,
    DateTime DueDate,
    bool IsOverdue);

public sealed record OverdueLoanReportDto(
    int LoanId,
    string UserName,
    string BookTitle,
    string InventoryCode,
    DateTime DueDate,
    int DaysLate);

public sealed record UserSummaryDto(int Id, string FullName, string Email, string UserType);

public sealed record ActiveLoanStaffDto(
    int LoanId,
    int UserId,
    string UserName,
    string BookTitle,
    string InventoryCode,
    DateTime DueDate);

public sealed record PenaltyReportDto(
    int PenaltyId,
    string UserName,
    string Reason,
    DateTime CreatedAt,
    DateTime? ActiveUntil,
    bool BlocksNewLoans,
    bool IsResolved);
