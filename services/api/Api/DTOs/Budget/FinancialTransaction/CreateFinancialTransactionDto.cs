namespace Api.DTOs.Budget.FinancialTransaction;

using Api.Models.Utils;

public class CreateFinancialTransactionDto
{
    public required string Name { get; set; }

    public decimal Amount { get; set; }

    public required ECurrency Currency { get; set; }

    public DateTime Date { get; set; }

    public Guid? CategoryId { get; set; } = null;

    public Guid? SubCategoryId { get; set; } = null;

    public string? Bank { get; set; } = null;
}