using AutoMapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.CustomMappers
{
    public class TeamViewMapperProfile : Profile
    {
        public TeamViewMapperProfile()
        {
            // ViewModel → Entity
            CreateMap<TeamViewModel, Team>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TeamLeadId, opt => opt.MapFrom(src => src.TeamLeadId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.DirectorId, opt => opt.MapFrom(src => src.DirectorId))
                .ForMember(dest => dest.TeamLead, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Director, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore());

            // Entity → ViewModel
            CreateMap<Team, TeamViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TeamLeadId, opt => opt.MapFrom(src => src.TeamLeadId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.DirectorId, opt => opt.MapFrom(src => src.DirectorId));
        }
    }
}
