namespace Sigebi.Desktop.Models;

public sealed class UserSummaryModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
}

public sealed class BookCatalogModel
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<CopyAvailabilityModel> Copies { get; set; } = new();
}

public sealed class CopyAvailabilityModel
{
    public int CopyId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public sealed class LoanRequestModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CopyId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string InventoryCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class ActiveLoanModel
{
    public int LoanId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string InventoryCode { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

public sealed class OverdueLoanModel
{
    public int LoanId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string InventoryCode { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysLate { get; set; }
}

public sealed class PenaltyModel
{
    public int PenaltyId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ActiveUntil { get; set; }
    public bool BlocksNewLoans { get; set; }
    public bool IsResolved { get; set; }
}

public sealed class ApiErrorModel
{
    public string? Error { get; set; }
}
