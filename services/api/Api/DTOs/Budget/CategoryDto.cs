namespace Api.DTOs.Budget;

public class CategoryDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public List<SubCategoryDto> SubCategories { get; set; } = [];
}