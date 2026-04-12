using Sigebi.Domain.Enums;

namespace Sigebi.Domain.Entities;

public class LoanRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;
    public LoanRequestStatus Status { get; set; } = LoanRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RejectionReason { get; set; }
}
