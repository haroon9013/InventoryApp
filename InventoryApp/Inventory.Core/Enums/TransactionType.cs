namespace Inventory.Core.Enums;

/// <summary>
/// Identifies the type of a stock movement recorded in StockTransaction.
/// Stored as int in the database.
/// 0=StockIn, 1=StockOut, 2=Adjustment
/// </summary>
public enum TransactionType
{
    StockIn = 0,
    StockOut = 1,
    Adjustment = 2
}
