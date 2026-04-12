using Sigebi.Domain.Enums;

namespace Sigebi.Domain.Entities;

public class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string InventoryCode { get; set; } = string.Empty;
    public CopyStatus Status { get; set; } = CopyStatus.Available;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
}
