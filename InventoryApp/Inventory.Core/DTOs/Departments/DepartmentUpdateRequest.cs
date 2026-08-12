namespace Inventory.Core.DTOs.Departments;

public sealed class DepartmentUpdateRequest
{
    public string DepartmentName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
