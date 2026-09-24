using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceSchool.Business.Events
{
    public record CourseCreatedEvent(Guid CourseId, DateTimeOffset CreatedAtUtc);
}
