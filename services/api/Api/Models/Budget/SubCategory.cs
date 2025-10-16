namespace Api.Models.Budget;

public class SubCategory
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? UserId { get; set; }
}