namespace Api.Mapping;

using AutoMapper;
using Api.DTOs.Budget;
using Api.DTOs.Budget.FinancialTransaction;
using Api.Models.Budget;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();

        CreateMap<SubCategory, SubCategoryDto>()
            .ForMember(dest => dest.IsCustom, opt => opt.MapFrom(src => src.UserId.HasValue));

        CreateMap<FinancialTransaction, FinancialTransactionDto>();
        CreateMap<CreateFinancialTransactionDto, FinancialTransaction>();
        CreateMap<FinancialTransactionPatchDto, FinancialTransaction>().ReverseMap();
    }
}
