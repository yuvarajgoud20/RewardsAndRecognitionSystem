using FluentValidation;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.FluentValidators
{
    public class YearQuarterViewValidator : AbstractValidator<YearQuarterViewModel>
    {
        public YearQuarterViewValidator()
        {
            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("Year is required.")
                .GreaterThan(0).WithMessage("Year is required.")
                .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

            RuleFor(x => x.Quarter)
                .NotNull().WithMessage("Quarter is required.");

            // Start date validation
            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("Start Date is required.")
                .Must((model, startDate) =>
                {
                    return startDate?.Year == model.Year;
                }).WithMessage("Start Date must be within the selected year.");

            // End date validation
            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("End Date is required.")
                .GreaterThan(x => x.StartDate)

                .WithMessage("End Date must be after Start Date.")
                .Must((model, endDate) =>
                {
                    return endDate?.Year == model.Year;
                }).WithMessage("End Date must be within the selected year.");
        }

    }
}

