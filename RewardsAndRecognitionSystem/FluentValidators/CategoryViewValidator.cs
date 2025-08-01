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
                .Matches(@"^(?=.*[A-Za-z])[A-Za-z _-]{2,}$").WithMessage("Category name must contain at least 2 characters and can only include Alphabets.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .Matches(@"^(?=.*[a-zA-Z]).+$").WithMessage("Description must contain at least one letter and cannot be only numbers.");
        }
    }
}
