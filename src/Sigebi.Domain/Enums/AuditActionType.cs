namespace Sigebi.Domain.Enums;

public enum AuditActionType
{
    LoanRequested = 0,
    LoanCreated = 1,
    LoanReturned = 2,
    PenaltyApplied = 3,
    InventoryChanged = 4,
    AccessDenied = 5,
    LoanRequestRejected = 6
}
