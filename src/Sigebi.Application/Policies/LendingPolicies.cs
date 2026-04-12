using Sigebi.Domain.Enums;

namespace Sigebi.Application.Policies;

public static class LendingPolicies
{
    public static int MaxConcurrentLoans(UserType type) => type switch
    {
        UserType.Student => 3,
        UserType.Teacher => 10,
        UserType.Staff => 8,
        _ => 3
    };

    public static int LoanDays(UserType type) => type switch
    {
        UserType.Student => 14,
        UserType.Teacher => 30,
        UserType.Staff => 21,
        _ => 14
    };

    public static int PenaltyBlockDays() => 7;
}
