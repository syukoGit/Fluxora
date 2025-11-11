namespace Api.Controllers.Budget;

using System.Security.Claims;
using System.Threading.Tasks;
using Api.Data;
using Api.DTOs.Budget.FinancialTransaction;
using Api.Models.Budget;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("api/budget/[controller]")]
public class FinancialTransactionsController(
    ApplicationDbContext dbContext,
    ILogger<FinancialTransactionsController> logger,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FinancialTransactionDto>), 200)]
    public IActionResult GetFinancialTransactions([FromQuery] DateTime? startDateTime,
                                                  [FromQuery] DateTime? endDateTime)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transactions = dbContext.Set<FinancialTransaction>()
                                        .AsNoTracking()
                                        .Where(ft => ft.UserId == userId)
                                        .AsEnumerable();

            if (startDateTime.HasValue)
            {
                transactions = transactions.Where(t => t.Date >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                transactions = transactions.Where(t => t.Date <= endDateTime.Value);
            }

            return Ok(transactions.Select(mapper.Map<FinancialTransactionDto>));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user ID from claims.");
            return Forbid();
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FinancialTransactionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFinancialTransactionAsync([FromRoute] Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transaction = await dbContext.Set<FinancialTransaction>()
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(ft => ft.Id == id && ft.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<FinancialTransactionDto>(transaction));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving financial transaction. {Message}", ex.Message);
            return Forbid();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(FinancialTransactionDto), 201)]
    public async Task<IActionResult> CreateFinancialTransactionAsync(
        [FromBody] CreateFinancialTransactionDto createTransactionDto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transaction = new FinancialTransaction
            {
                UserId = userId,
                Amount = createTransactionDto.Amount,
                Currency = createTransactionDto.Currency,
                Date = createTransactionDto.Date,
                Name = createTransactionDto.Name,
                CategoryId = createTransactionDto.CategoryId,
                SubCategoryId = createTransactionDto.SubCategoryId,
                Bank = createTransactionDto.Bank,
            };

            var result = await dbContext.Set<FinancialTransaction>().AddAsync(transaction);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFinancialTransactions), new { id = result.Entity.Id },
                                   mapper.Map<FinancialTransactionDto>(result.Entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating financial transaction. {Message}", ex.Message);
            return Forbid();
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(FinancialTransactionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateFinancialTransactionAsync([FromRoute] Guid id,
                                                                     [FromBody]
                                                                     JsonPatchDocument<FinancialTransactionPatchDto>
                                                                         patchDoc)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transaction = await dbContext.Set<FinancialTransaction>()
                                             .FirstOrDefaultAsync(ft => ft.Id == id && ft.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            var transactionToPatch = mapper.Map<FinancialTransactionPatchDto>(transaction);

            patchDoc.ApplyTo(transactionToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    logger.LogWarning("ModelState validation error: {ErrorMessage} - {Exception}", error.ErrorMessage,
                                      error.Exception);
                }

                return BadRequest(new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList(),
                });
            }

            mapper.Map(transactionToPatch, transaction);

            await dbContext.SaveChangesAsync();

            return Ok(mapper.Map<FinancialTransactionDto>(transaction));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating financial transaction. {Message}", ex.Message);
            return Forbid();
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteFinancialTransactionAsync([FromRoute] Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transaction = await dbContext.Set<FinancialTransaction>()
                                             .FirstOrDefaultAsync(ft => ft.Id == id && ft.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            dbContext.Set<FinancialTransaction>().Remove(transaction);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting financial transaction. {Message}", ex.Message);
            return Forbid();
        }
    }
}