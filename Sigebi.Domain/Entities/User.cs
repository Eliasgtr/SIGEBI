using Sigebi.Domain.Enums;

namespace Sigebi.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
    public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
}
