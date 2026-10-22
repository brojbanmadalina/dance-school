using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DanceSchool.Api.Consumers;
using DanceSchool.Api.Hubs;
using DanceSchool.Api.Services;
using DanceSchool.Api.Configurations;
using DanceSchool.Api.Extensions;
using DanceSchool.Business.Extensions;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Validators.Auth;
using DanceSchool.DataAccess.Data;
using DanceSchool.Services;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// servicii de baza
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache(); //cache in RAM, folosit in CourseController v2

// Versionare API (vezi CourseController/AttendanceController)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger (câte un document per versiune)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DanceSchool API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "DanceSchool API", Version = "v2" });
});

// PostgreSQL DbContext
builder.Services.AddDbContext<DanceSchoolDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
);

// JWT
builder.Services.AddJwt();

// Servicii business + configurare/infrastructura API
builder.Services.AddAppServices();
builder.Services.AddAppConfiguration(builder.Configuration, builder.Environment);

// RabbitMQ + MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CourseCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.ConfigureEndpoints(context);
    });
});

// IEventPublisher = abstractizare peste MassTransit, ca CourseService să nu depindă direct de RabbitMQ
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// Health checks (folosit de Aspire Dashboard)
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Quartz (job-uri programate)
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

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

// SignalR (chat)
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

//Swagger UI doar în Development
if (app.Environment.IsDevelopment())
{
    var apiVersionDescProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescProvider.ApiVersionDescriptions)
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
    });
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();

app.MapHealthChecks("/health");
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