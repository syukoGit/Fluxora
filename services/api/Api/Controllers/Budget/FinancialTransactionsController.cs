namespace Api.Controllers.Budget;

using Api.Models.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("api/budget/[controller]")]
public class FinancialTransactionsController(DbContext dbContext, ILogger<FinancialTransactionsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FinancialTransaction>), 200)]
    public IActionResult GetFinancialTransactions()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found"));

            var transactions = dbContext.Set<FinancialTransaction>().AsNoTracking().Where(ft => ft.UserId == userId).ToList();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user ID from claims.");
            return Forbid();
        }
    }
}