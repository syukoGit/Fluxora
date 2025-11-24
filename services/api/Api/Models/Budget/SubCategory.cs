namespace Api.Models.Budget;

public class SubCategory
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid CategoryId { get; set; }

    /// <summary>
    /// Foreign key to User table (Keycloak user ID as Guid).
    /// Null for default/system sub-categories that belong to no specific user.
    /// </summary>
    public Guid? UserId { get; set; }
}