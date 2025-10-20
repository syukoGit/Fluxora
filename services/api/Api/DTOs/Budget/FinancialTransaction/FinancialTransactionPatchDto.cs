namespace Api.DTOs.Budget.FinancialTransaction;

using Api.Models.Utils;

public class FinancialTransactionPatchDto
{
    public string? Name { get; set; }

    public decimal? Amount { get; set; }

    public ECurrency? Currency { get; set; }

    public DateTime? DateTime { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SubCategoryId { get; set; }

    public string? Bank { get; set; }
}