using DanceSchool.Api.Hubs;
using DanceSchool.Api.Services;
using DanceSchool.Business.Configurations;
using DanceSchool.Business.Extensions;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Validators.Auth;
using DanceSchool.DataAccess.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL DbContext
builder.Services.AddDbContext<DanceSchoolDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
);

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.AddJwt();

//Services
builder.Services.AddAppServices(builder.Configuration, builder.Environment);
builder.Services.AddScoped<IUserProvider, UserProvider>();

//job
builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UseProperties = true;
        store.UsePostgres(pg =>
        {
            pg.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            pg.TablePrefix = "quartz.qrtz_";
        });
        store.UseSystemTextJsonSerializer();
    });
    var jobKey = new JobKey("CourseReminderJob");
    q.AddJob<CourseReminderJob>(opts => opts.WithIdentity(jobKey).StoreDurably());
    q.AddTrigger(opts =>
        opts.ForJob(jobKey)
            .WithIdentity("CourseReminderTrigger")
            .WithCronSchedule(
                "0 0 8 * * ?",
                x =>
                {
                    x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest"));
                    x.WithMisfireHandlingInstructionFireAndProceed();
                }
            )
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddQuartzDashboard();

//Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

// SignalR
builder.Services.AddSignalR();

// CORS pentru chat test (HTML deschis ca fisier local)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatTest", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();

app.UseCors("ChatTest");

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
//mapam chathub-ul
app.MapHub<ChatHub>("/chatHub");

// Quartz Dashboard
app.MapQuartzDashboard();

app.Run();