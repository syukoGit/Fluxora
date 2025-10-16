namespace Api.Extensions;

using Api.DTOs.Budget;
using Api.Models.Budget;

public static class DtoExtension
{
    public static IEnumerable<TDestination> Transform<TSource, TDestination>(this IEnumerable<TSource> source, Func<TSource, TDestination> transformer)
    {
        foreach (var item in source)
        {
            yield return transformer(item);
        }
    }

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SubCategories = category.SubCategories.Select(sc => sc.ToDto()).ToList()
        };
    }

    public static SubCategoryDto ToDto(this SubCategory subCategory)
    {
        return new SubCategoryDto
        {
            Id = subCategory.Id,
            Name = subCategory.Name,
            IsCustom = subCategory.UserId.HasValue
        };
    }
}