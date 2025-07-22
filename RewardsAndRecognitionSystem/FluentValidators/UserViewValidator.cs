using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class UserViewValidator : AbstractValidator<UserViewModel>
    {
        public UserViewValidator()
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.PasswordHash)
    .NotEmpty().WithMessage("Password is required.")
    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$")
    .WithMessage("Password must be at least 6 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one special character.");


            RuleFor(x => x.SelectedRole)
                .NotEmpty().WithMessage("Please select a role.");

        }

    }
}
