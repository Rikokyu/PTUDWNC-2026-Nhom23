using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator
    : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Bio)
            .MaximumLength(1000);

        RuleFor(command => command.AvatarUrl)
            .MaximumLength(500);
    }
}