namespace Api.DTOs.Budget.FinancialTransaction;

using Api.Models.Utils;

public class FinancialTransactionDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public decimal Amount { get; set; }

    public ECurrency Currency { get; set; }

    public DateTime Date { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SubCategoryId { get; set; }

    public string? Bank { get; set; }
}