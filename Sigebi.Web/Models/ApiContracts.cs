namespace Sigebi.Web.Models;

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

public sealed class UserSummaryModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
}

public sealed class UserLoanModel
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string InventoryCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
}

public sealed class ApiErrorModel
{
    public string? Error { get; set; }
}
