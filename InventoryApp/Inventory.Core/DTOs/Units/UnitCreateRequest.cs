namespace Inventory.Core.DTOs.Units;

public sealed class UnitCreateRequest
{
    public string UnitName { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
}
