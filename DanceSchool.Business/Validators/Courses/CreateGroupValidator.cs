using DanceSchool.Business.Models.Courses;
using FluentValidation;

namespace DanceSchool.Business.Validators.Courses
{
    public class CreateGroupValidator : AbstractValidator<CreateGroupRequest>
    {
        public CreateGroupValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty();

            RuleFor(x => x.InstructorId)
                .NotEmpty();

            RuleFor(x => x.Level)
                .NotEmpty()
                .Must(l => new[] { "Beginner", "Intermediate", "Advanced" }.Contains(l))
                .WithMessage("Level must be Beginner, Intermediate or Advanced");

            RuleFor(x => x.Room)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Location)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0)
                .LessThanOrEqualTo(50);
        }
    }
}