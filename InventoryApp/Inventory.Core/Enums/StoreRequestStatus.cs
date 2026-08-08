namespace Inventory.Core.Enums;

/// <summary>
/// Represents the lifecycle status of a Store Request.
/// Stored as int in the database.
/// 0=Pending, 1=Approved, 2=Rejected, 3=Issued, 4=Cancelled
/// </summary>
public enum StoreRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Issued = 3,
    Cancelled = 4
}
