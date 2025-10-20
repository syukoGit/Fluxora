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
    public IActionResult GetFinancialTransactions()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? throw new Exception("User ID not found"));

            var transactions = dbContext.Set<FinancialTransaction>()
                                        .AsNoTracking()
                                        .Where(ft => ft.UserId == userId)
                                        .AsEnumerable()
                                        .Select(mapper.Map<FinancialTransactionDto>)
                                        .ToList();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user ID from claims.");
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
                DateTime = createTransactionDto.DateTime,
                Name = createTransactionDto.Name,
                CategoryId = createTransactionDto.CategoryId,
                SubCategoryId = createTransactionDto.SubCategoryId,
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
                return BadRequest(ModelState);
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
}