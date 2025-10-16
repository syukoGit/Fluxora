namespace Api.DTOs.Budget;

public class CreateSubCategoryDto
{
    public required string Name { get; set; }

    public required Guid CategoryId { get; set; }
}