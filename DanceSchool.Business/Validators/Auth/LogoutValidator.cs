using DanceSchool.Business.Models.Auth;
using FluentValidation;

namespace DanceSchool.Business.Validators.Auth
{
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
