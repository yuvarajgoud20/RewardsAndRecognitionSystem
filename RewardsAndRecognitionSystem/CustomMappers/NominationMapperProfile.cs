using AutoMapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.CustomMappers
{
    public class NominationMapperProfile : Profile
    {
        public NominationMapperProfile()
        {
            // ViewModel → Entity
            CreateMap<NominationViewModel, Nomination>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NominatorId, opt => opt.MapFrom(src => src.NominatorId))
                .ForMember(dest => dest.NomineeId, opt => opt.MapFrom(src => src.NomineeId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Achievements, opt => opt.MapFrom(src => src.Achievements))
                .ForMember(dest => dest.YearQuarterId, opt => opt.MapFrom(src => src.YearQuarterId))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // set in controller
                .ForMember(dest => dest.Status, opt => opt.Ignore())    // set in controller
                .ForMember(dest => dest.Nominator, opt => opt.Ignore())
                .ForMember(dest => dest.Nominee, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.YearQuarter, opt => opt.Ignore())
                .ForMember(dest => dest.Approvals, opt => opt.Ignore());

            // Entity → ViewModel
            CreateMap<Nomination, NominationViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NominatorId, opt => opt.MapFrom(src => src.NominatorId))
                .ForMember(dest => dest.NomineeId, opt => opt.MapFrom(src => src.NomineeId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Achievements, opt => opt.MapFrom(src => src.Achievements))
                .ForMember(dest => dest.YearQuarterId, opt => opt.MapFrom(src => src.YearQuarterId));
        
        }
    }
}
