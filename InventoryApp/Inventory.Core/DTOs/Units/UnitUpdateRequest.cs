namespace Inventory.Core.DTOs.Units;

public sealed class UnitUpdateRequest
{
    public string UnitName { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
