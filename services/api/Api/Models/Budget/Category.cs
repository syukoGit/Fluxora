namespace Api.Models.Budget;

public class Category
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public List<SubCategory> SubCategories { get; set; } = [];
}