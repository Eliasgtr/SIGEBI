namespace Sigebi.Domain.Entities;

public class Penalty
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? LoanId { get; set; }
    public Loan? Loan { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ActiveUntil { get; set; }
    public bool BlocksNewLoans { get; set; }
    public bool IsResolved { get; set; }
}
