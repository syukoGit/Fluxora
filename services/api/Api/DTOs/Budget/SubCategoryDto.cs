namespace Api.DTOs.Budget;

public class SubCategoryDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public bool IsCustom { get; set; }
}