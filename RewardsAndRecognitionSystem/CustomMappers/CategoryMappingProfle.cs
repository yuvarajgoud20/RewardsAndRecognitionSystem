using AutoMapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.CustomMappers
{
    public class CategoryMappingProfle : Profile
    {
        public CategoryMappingProfle() 
        {
            // ViewModel → Entity
            CreateMap<CategoryViewModel, Category>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.isActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Nominations, opt => opt.Ignore()); 

            // Entity → ViewModel
            CreateMap<Category, CategoryViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.isActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
