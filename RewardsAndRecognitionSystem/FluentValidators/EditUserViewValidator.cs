using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class EditUserViewValidator : AbstractValidator<EditUserViewModel>
    {
        public EditUserViewValidator()
        {

            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("Name is required.")
               .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
               .Matches(@"^(?=.*[A-Za-z])[A-Za-z0-9\s'-]+$")
               .WithMessage("Name can contain letters, numbers, spaces, hyphens, or apostrophes, but must include at least one letter.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");



        }
    }
}
