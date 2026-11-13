using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator
    : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(command => command.Bio)
            .MaximumLength(500);

        RuleFor(command => command.AvatarUrl)
            .MaximumLength(500);
    }
}
