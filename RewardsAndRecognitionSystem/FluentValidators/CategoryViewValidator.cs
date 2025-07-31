using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class CategoryViewValidator : AbstractValidator<CategoryViewModel>
    {
        public CategoryViewValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
                .Matches(@"^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-\&]+$").WithMessage("Category name must contain at least one letter and can only contain letters, numbers, spaces, hyphens, and ampersands.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .Matches(@"^(?=.*[a-zA-Z]).+$").WithMessage("Description must contain at least one letter and cannot be only numbers.");
        }
    }
}
