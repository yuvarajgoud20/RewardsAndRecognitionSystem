using AutoMapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.CustomMappers
{
    public class EditUserMappingProfile : Profile
    {
        public EditUserMappingProfile()
        {
            CreateMap<EditUserViewModel, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Identity requires UserName
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId.HasValue ? new Guid(src.TeamId.Value.ToString()) : (Guid?)null))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be set manually after hashing
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Identity sets this
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.NominationsGiven, opt => opt.Ignore())
                .ForMember(dest => dest.NominationsReceived, opt => opt.Ignore())
                .ForMember(dest => dest.Approvals, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Optional: reverse map if needed
            CreateMap<User, EditUserViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString())) // Convert Guid to string
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId));
               
        }
    }
}
