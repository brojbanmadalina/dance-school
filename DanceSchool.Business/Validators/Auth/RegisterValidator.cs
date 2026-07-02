using DanceSchool.Business.Models.Auth;
using DanceSchool.DataAccess.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Validators.Auth
{
    public class RegisterValidator : AbstractValidator<RegisterUserRequest>
    {
        private readonly DanceSchoolDbContext _db;

        public RegisterValidator(DanceSchoolDbContext db)
        {
            _db = db;
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(BeUniqueEmail)
                .WithMessage("Email already exists")
                .MaximumLength(100);

            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number");
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            email = email.Trim().ToLower();

            return !await _db.Users.AnyAsync(x => x.Email.ToLower() == email, ct);
        }
    }
}
