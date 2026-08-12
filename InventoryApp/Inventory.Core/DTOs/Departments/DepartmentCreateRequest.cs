namespace Inventory.Core.DTOs.Departments;

public sealed class DepartmentCreateRequest
{
    public string DepartmentName { get; init; } = string.Empty;
    public string? Description { get; init; }
}
