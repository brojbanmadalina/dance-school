//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;

//namespace DanceSchool.DataAccess.Data
//{
//    public class DanceSchoolDbContextFactory : IDesignTimeDbContextFactory<DanceSchoolDbContext>
//    {
//        public DanceSchoolDbContext CreateDbContext(string[] args)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DanceSchool"))
//                .AddJsonFile("appsettings.json")
//                .Build();

//            var optionsBuilder = new DbContextOptionsBuilder<DanceSchoolDbContext>();
//            optionsBuilder
//                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
//                .UseSnakeCaseNamingConvention();

//            return new DanceSchoolDbContext(optionsBuilder.Options);
//        }
//    }
//}