namespace Api.Models.Budget;

using System.Text.Json;

/// <summary>
/// Default categories and subcategories loaded from JSON for seeding.
/// </summary>
public static class DefaultCategories
{
    private static readonly Dictionary<string, List<string>> s_categoriesData;
    private static readonly Dictionary<string, Guid> s_categoryIds;
    private static readonly List<(string CategoryName, string SubCategoryName, Guid Id)> s_subCategoryData;

    static DefaultCategories()
    {
        // Load JSON file
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "default-categories.json");
        var jsonContent = File.ReadAllText(jsonPath);
        s_categoriesData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent)
            ?? throw new InvalidOperationException("Failed to load default categories JSON");

        // Generate deterministic GUIDs for categories
        s_categoryIds = [];
        int categoryIndex = 1;
        foreach (var categoryName in s_categoriesData.Keys.OrderBy(k => k))
        {
            s_categoryIds[categoryName] = Guid.Parse($"00000000-0000-0000-0000-{categoryIndex:D12}");
            categoryIndex++;
        }

        // Generate deterministic GUIDs for subcategories
        s_subCategoryData = [];
        int subCategoryIndex = 1;
        foreach (var (categoryName, subCategories) in s_categoriesData.OrderBy(kv => kv.Key))
        {
            foreach (var subCategoryName in subCategories)
            {
                var subCategoryId = Guid.Parse($"00000000-0000-0000-0001-{subCategoryIndex:D12}");
                s_subCategoryData.Add((categoryName, subCategoryName, subCategoryId));
                subCategoryIndex++;
            }
        }
    }

    public static Category[] GetCategories() =>
        [.. s_categoryIds.Select(kvp => new Category
        {
            Id = kvp.Value,
            Name = kvp.Key
        })];

    public static SubCategory[] GetSubCategories() =>
        [.. s_subCategoryData.Select(item => new SubCategory
        {
            Id = item.Id,
            Name = item.SubCategoryName,
            CategoryId = s_categoryIds[item.CategoryName],
            UserId = null
        })];
}
