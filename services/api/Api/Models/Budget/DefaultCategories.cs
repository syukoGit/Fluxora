namespace Api.Models.Budget;

using System.Text.Json;

/// <summary>
/// Default categories and subcategories loaded from JSON for seeding.
/// </summary>
public static class DefaultCategories
{
    private static readonly Dictionary<string, Guid> s_categoryIds;

    private static readonly List<( string CategoryName, string SubCategoryName, Guid Id )> s_subCategoryData;

    static DefaultCategories()
    {
        // Load JSON file
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "default-categories.json");
        string jsonContent = File.ReadAllText(jsonPath);

        var categoriesData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent)
                          ?? throw new InvalidOperationException("Failed to load default categories JSON");

        // Generate deterministic GUIDs for categories
        s_categoryIds = [];
        var categoryIndex = 1;

        foreach (string categoryName in categoriesData.Keys.OrderBy(k => k))
        {
            s_categoryIds[categoryName] = Guid.Parse($"00000000-0000-0000-0000-{categoryIndex:D12}");
            categoryIndex++;
        }

        // Generate deterministic GUIDs for subcategories
        s_subCategoryData = [];
        var subCategoryIndex = 1;

        foreach ((string categoryName, var subCategories) in categoriesData.OrderBy(kv => kv.Key))
        {
            foreach (string subCategoryName in subCategories)
            {
                var subCategoryId = Guid.Parse($"00000000-0000-0000-0001-{subCategoryIndex:D12}");
                s_subCategoryData.Add((categoryName, subCategoryName, subCategoryId));
                subCategoryIndex++;
            }
        }
    }

    public static Category[] GetCategories()
    {
        return [.. s_categoryIds.Select(kvp => new Category { Id = kvp.Value, Name = kvp.Key })];
    }

    public static SubCategory[] GetSubCategories()
    {
        return
        [
            .. s_subCategoryData.Select(item => new SubCategory
            {
                Id = item.Id,
                Name = item.SubCategoryName,
                CategoryId = s_categoryIds[item.CategoryName],
                UserId = null,
            }),
        ];
    }
}
