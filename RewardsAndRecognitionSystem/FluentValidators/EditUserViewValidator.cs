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
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            

        }
    }
}
