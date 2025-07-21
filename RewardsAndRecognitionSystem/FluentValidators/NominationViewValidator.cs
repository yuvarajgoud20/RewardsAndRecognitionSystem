using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class NominationViewValidator : AbstractValidator<NominationViewModel>
    {
        public NominationViewValidator()
        {
            RuleFor(x => x.NominatorId)
                .NotEmpty().WithMessage("Nominator is required.");

            RuleFor(x => x.NomineeId)
                .NotEmpty().WithMessage("Nominee is required.")
                .NotEqual(x => x.NominatorId).WithMessage("Nominator and Nominee cannot be the same user.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.Achievements)
                .NotEmpty().WithMessage("Achievements are required.")
                .MaximumLength(1000).WithMessage("Achievements must not exceed 1000 characters.");

            RuleFor(x => x.YearQuarterId)
                .NotEmpty().WithMessage("Year and Quarter selection is required.");
        }
    }
}
