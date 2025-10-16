namespace Api.Controllers.Categories;

using System.Security.Claims;
using Api.Data;
using Api.DTOs.Budget;
using Api.Extensions;
using Api.Models.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BudgetController(ApplicationDbContext dbContext, ILogger<BudgetController> logger) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<BudgetController> _logger = logger;

    [Authorize]
    [HttpGet("Categories")]
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
                                   .Include(c => c.SubCategories.Where(sc => sc.UserId == userId || sc.UserId == null))
                                   .Transform(c => c.ToDto())
                                   .ToList();

        return Ok(categories);
    }

    [Authorize]
    [HttpGet("Categories/{id:guid}")]
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
    [HttpPost("Categories")]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(403)]
    public IActionResult CreateCategory([FromBody] CreateSubCategoryDto createSubCategoryDto)
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

        var subCategory = new SubCategory
        {
            Name = createSubCategoryDto.Name,
            CategoryId = createSubCategoryDto.CategoryId,
            UserId = userId
        };

        _dbContext.Set<SubCategory>().Add(subCategory);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetCategory), new { id = subCategory.Id }, subCategory.ToDto());
    }
}