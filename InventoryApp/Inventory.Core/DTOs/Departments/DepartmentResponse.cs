namespace Inventory.Core.DTOs.Departments;

public sealed class DepartmentResponse
{
    public int Id { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
