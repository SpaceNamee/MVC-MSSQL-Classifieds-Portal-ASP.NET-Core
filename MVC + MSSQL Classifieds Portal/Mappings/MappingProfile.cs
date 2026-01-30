using AutoMapper;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Models.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Listing, ListingViewModel>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.OwnerUsername, o => o.MapFrom(s => s.User.Username))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId));

        CreateMap<ListingCreateViewModel, Listing>();
    }
}