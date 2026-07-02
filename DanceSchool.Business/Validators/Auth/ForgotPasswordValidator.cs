using DanceSchool.Business.Models.Auth;
using FluentValidation;

namespace DanceSchool.Business.Validators.Auth
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
