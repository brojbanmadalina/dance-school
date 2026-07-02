//using System.Data.Common;
//using DanceSchool.DataAccess;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;

//namespace DanceSchool.Integration.Tests
//{
//    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
//        where TProgram : class
//    {
//        private string _dbPath;

//        protected override void ConfigureWebHost(IWebHostBuilder builder)
//        {
//            builder.ConfigureServices(services =>
//            {
//                var dbContextDescriptor = services.SingleOrDefault(d =>
//                    d.ServiceType == typeof(IDbContextOptionsConfiguration<DanceSchoolDbContext>)
//                );

//                services.Remove(dbContextDescriptor);

//                var dbConnectionDescriptor = services.SingleOrDefault(d =>
//                    d.ServiceType == typeof(DbConnection)
//                );

//                services.Remove(dbConnectionDescriptor);
//                _dbPath = Path.Combine(
//                    Path.GetTempPath(),
//                    $"danceSchool_test_{Guid.NewGuid():N}.db"
//                );

//                services.AddDbContext<DanceSchoolDbContext>(
//                    (container, options) =>
//                    {
//                        options.UseSqlite($"Data Source={_dbPath};Pooling=False");
//                    }
//                );
//                var sp = services.BuildServiceProvider();
//                using (var scope = sp.CreateScope())
//                {
//                    var scopedServices = scope.ServiceProvider;
//                    var db = scopedServices.GetRequiredService<DanceSchoolDbContext>();

//                    db.Database.EnsureCreated();
//                }
//            });

//            builder.UseEnvironment("Development");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (File.Exists(_dbPath))
//            {
//                File.Delete(_dbPath);
//            }

//            base.Dispose(disposing);
//        }
//    }
//}
