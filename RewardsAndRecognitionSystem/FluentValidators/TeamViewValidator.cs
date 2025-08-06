using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class TeamViewValidator : AbstractValidator<TeamViewModel>
    {
        public TeamViewValidator()
        {
            RuleFor(x => x.Name)
             .NotEmpty().WithMessage("Team name is required.")
             .MaximumLength(100).WithMessage("Team name must not exceed 100 characters.")
             .Matches(@"^(?=.*[A-Za-z])[A-Za-z _-]{2,}$").WithMessage("Team name must contain at least 2 characters and can only include Alphabets.");

            RuleFor(x => x.TeamLeadId)
                .NotEmpty().WithMessage("Team Lead is required.");

            RuleFor(x => x.ManagerId)
                .NotEmpty().WithMessage("Manager is required.");

            RuleFor(x => x.DirectorId)
                .NotEmpty().WithMessage("Director is required.");
        }
    }
}
