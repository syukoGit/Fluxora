namespace Api.DTOs.Budget.FinancialTransaction;

using Api.Models.Utils;

public class CreateFinancialTransactionDto
{
    public required string Name { get; set; }

    public decimal Amount { get; set; }

    public required ECurrency Currency { get; set; }

    public DateTime DateTime { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SubCategoryId { get; set; }

    public string? Bank { get; set; }
}