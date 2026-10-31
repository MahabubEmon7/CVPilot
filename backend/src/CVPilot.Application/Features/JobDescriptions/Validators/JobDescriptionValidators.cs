using CVPilot.Application.Features.JobDescriptions.DTOs;
using FluentValidation;

namespace CVPilot.Application.Features.JobDescriptions.Validators;

public class CreateJobDescriptionRequestValidator : AbstractValidator<CreateJobDescriptionRequestDto>
{
    public CreateJobDescriptionRequestValidator()
    {
        RuleFor(x => x.RawContent)
            .NotEmpty().WithMessage("Job circular description or text is required.")
            .MinimumLength(50).WithMessage("Job description is too short. Please provide at least 50 characters for meaningful analysis.")
            .MaximumLength(30000).WithMessage("Job description exceeds the maximum permitted length of 30,000 characters.");

        RuleFor(x => x.JobTitle)
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(150).WithMessage("Company name must not exceed 150 characters.");
    }
}

public class UpdateJobDescriptionRequestValidator : AbstractValidator<UpdateJobDescriptionRequestDto>
{
    public UpdateJobDescriptionRequestValidator()
    {
        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(150).WithMessage("Company name must not exceed 150 characters.");
    }
}
