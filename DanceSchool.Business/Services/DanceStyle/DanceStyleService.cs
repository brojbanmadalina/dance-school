using DanceSchool.Business.Models.DanceStyle;
using DanceSchool.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.DanceStyle
{
    public class DanceStyleService
    {
        private readonly DanceSchoolDbContext _db;

        public DanceStyleService(DanceSchoolDbContext db)
        {
            _db = db;
        }

        public async Task<List<DanceStyleData>> GetAll()
        {
            return await _db
                .DanceStyles.Select(x => new DanceStyleData { Id = x.Id, Name = x.Name })
                .ToListAsync();
        }
    }
}
