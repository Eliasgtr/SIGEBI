using Sigebi.Domain.Enums;

namespace Sigebi.Domain.Entities;

public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
}
