namespace Api.Controllers.Budget;

using System.Security.Claims;
using System.Threading.Tasks;
using Api.DTOs.Budget;
using AutoMapper;
using Api.Models.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;

[ApiController]
[Route("api/budget/[controller]")]
public class CategoriesController(ApplicationDbContext dbContext, ILogger<CategoriesController> logger, IMapper mapper)
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), 200)]
    [ProducesResponseType(403)]
    public IActionResult GetCategories()
    {
        Guid userId;

        try
        {
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? throw new Exception("User ID not found");

            userId = Guid.Parse(userIdString);
        }
        catch (Exception)
        {
            return Forbid();
        }

        var categories = dbContext.Set<Category>()
                                  .AsNoTracking()
                                  .Include(c => c.SubCategories.Where(sc => sc.UserId == userId || sc.UserId == null))
                                  .ToList()
                                  .Select(mapper.Map<CategoryDto>)
                                  .ToList();

        return Ok(categories);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public IActionResult GetCategory(Guid id)
    {
        Guid userId;

        try
        {
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? throw new Exception("User ID not found");

            userId = Guid.Parse(userIdString);
        }
        catch (Exception)
        {
            logger.LogError("Error occurred while retrieving category with ID {CategoryId}.", id);
            logger.LogError("User ID could not be determined from the token.");

            return Forbid();
        }

        var category = dbContext.Set<Category>()
                                .AsNoTracking()
                                .Include(c => c.SubCategories.Where(sc => sc.UserId == userId || sc.UserId == null))
                                .FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<CategoryDto>(category));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(typeof(ForbidResult), 403)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateSubCategoryDto createSubCategoryDto)
    {
        Guid userId;

        try
        {
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? throw new Exception("User ID not found");

            userId = Guid.Parse(userIdString);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occurred while creating subcategory. Message: {Message}", ex.Message);

            return Forbid();
        }

        if (!await dbContext.Set<Category>().AsNoTracking().AnyAsync(c => c.Id == createSubCategoryDto.CategoryId))
        {
            return BadRequest("Invalid CategoryId");
        }

        if (await dbContext.Set<SubCategory>()
                           .AsNoTracking()
                           .AnyAsync(sc => sc.Name == createSubCategoryDto.Name
                                        && sc.CategoryId == createSubCategoryDto.CategoryId
                                        && (sc.UserId == userId || sc.UserId == null)))
        {
            return Conflict("A sub-category with the same name already exists in this category.");
        }

        var subCategory = new SubCategory
        {
            Name = createSubCategoryDto.Name, CategoryId = createSubCategoryDto.CategoryId, UserId = userId,
        };

        var newSubCategory = await dbContext.Set<SubCategory>().AddAsync(subCategory);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = newSubCategory.Entity.Id },
                               mapper.Map<SubCategoryDto>(newSubCategory.Entity));
    }
}