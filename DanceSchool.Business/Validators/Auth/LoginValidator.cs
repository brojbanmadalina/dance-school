using DanceSchool.Business.Models.Auth;
using FluentValidation;

namespace DanceSchool.Business.Validators.Auth
{
    public class LoginValidator : AbstractValidator<LoginUserRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
