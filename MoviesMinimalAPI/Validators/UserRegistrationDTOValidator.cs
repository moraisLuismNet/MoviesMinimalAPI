using FluentValidation;
using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Validators
{
    public class UserRegistrationDTOValidator : AbstractValidator<UserRegistrationDTO>
    {
        public UserRegistrationDTOValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}
