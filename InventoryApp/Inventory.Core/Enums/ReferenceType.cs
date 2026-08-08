namespace Inventory.Core.Enums;

/// <summary>
/// Identifies the source document type for a StockTransaction entry.
/// Stored as int in the database.
/// 0=Purchase, 1=StoreRequest, 2=StockIssue
/// </summary>
public enum ReferenceType
{
    Purchase = 0,
    StoreRequest = 1,
    StockIssue = 2
}
