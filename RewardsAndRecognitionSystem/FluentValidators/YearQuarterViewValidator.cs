using FluentValidation;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionSystem.ViewModels;
using System;

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

            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("Start Date is required.")
                .Must((model, startDate) =>
                {
                    return startDate?.Year == model.Year;
                }).WithMessage("Start Date must be within the selected year.");

            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("End Date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End Date must be after Start Date.")
                .Must((model, endDate) =>
                {
                    return endDate?.Year == model.Year;
                }).WithMessage("End Date must be within the selected year.");

            // ✅ Enforce exact 3-month (approx. quarter) range
            RuleFor(x => x.EndDate)
       .Must((model, endDate) =>
       {
           if (model.StartDate == null || endDate == null)
               return true; // Skip this check if dates are missing

           var diffInDays = (endDate.Value - model.StartDate.Value).TotalDays + 1;
           return diffInDays >= 89 && diffInDays <= 92;
       })
       .WithMessage("Date range must be approximately 3 months long.")
       .When(x => x.StartDate != null && x.EndDate != null); // ✅ Add this guard

        }
    }
}
