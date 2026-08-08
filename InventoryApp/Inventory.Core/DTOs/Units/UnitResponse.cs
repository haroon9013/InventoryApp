namespace Inventory.Core.DTOs.Units;

public sealed class UnitResponse
{
    public int Id { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
