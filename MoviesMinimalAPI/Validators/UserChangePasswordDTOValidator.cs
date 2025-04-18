using FluentValidation;
using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Validators
{
    public class UserChangePasswordDTOValidator : AbstractValidator<UserChangePasswordDTO>
    {
        public UserChangePasswordDTOValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        }
    }
}
