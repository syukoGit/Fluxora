namespace Api.Controllers.Budget;

using System.Security.Claims;
using System.Threading.Tasks;
using Api.Data;
using Api.DTOs.Budget;
using Api.Extensions;
using Api.Models.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/budget/[controller]")]
public class CategoriesController(ApplicationDbContext dbContext, ILogger<CategoriesController> logger) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<CategoriesController> _logger = logger;

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), 200)]
    [ProducesResponseType(403)]
    public IActionResult GetCategories()
    {
        Guid userId;

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found");
            userId = Guid.Parse(userIdString);
        }
        catch (Exception)
        {
            return Forbid();
        }

        var categories = _dbContext.Set<Category>()
                                   .AsNoTracking()
                                   .Include(c => c.SubCategories.Where(sc => sc.UserId == userId || sc.UserId == null))
                                   .Transform(c => c.ToDto())
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found");
            userId = Guid.Parse(userIdString);
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while retrieving category with ID {CategoryId}. User ID not found in claims.", id);
            return Forbid();
        }

        var category = _dbContext.Set<Category>()
                                 .AsNoTracking()
                                 .Include(c => c.SubCategories.Where(sc => sc.UserId == userId || sc.UserId == null))
                                 .FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category.ToDto());
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found");
            userId = Guid.Parse(userIdString);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while creating subcategory. Message: {Message}", ex.Message);
            return Forbid();
        }

        if (!await _dbContext.Set<Category>().AsNoTracking().AnyAsync(c => c.Id == createSubCategoryDto.CategoryId))
        {
            return BadRequest("Invalid CategoryId");
        }

        if (await _dbContext.Set<SubCategory>().AsNoTracking().AnyAsync(sc =>
                sc.Name == createSubCategoryDto.Name &&
                sc.CategoryId == createSubCategoryDto.CategoryId &&
                (sc.UserId == userId || sc.UserId == null)))
        {
            return Conflict("A sub-category with the same name already exists in this category.");
        }

        var subCategory = new SubCategory
        {
            Name = createSubCategoryDto.Name,
            CategoryId = createSubCategoryDto.CategoryId,
            UserId = userId
        };

        await _dbContext.Set<SubCategory>().AddAsync(subCategory);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = subCategory.Id }, subCategory.ToDto());
    }
}