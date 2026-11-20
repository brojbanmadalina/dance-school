using DanceSchool.Business.Extensions;
using DanceSchool.Business.Validators.Auth;
using DanceSchool.Controllers.Chat;
using DanceSchool.Extensions;
using FluentValidation;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache(); //cache in RAM, folosit in CourseController v2
builder.Services.AddApiDocumentation();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddJwt();
builder.Services.AddAuthorization();

builder.Services.AddAppServices();
builder.Services.AddAppConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScheduling(builder.Configuration);
builder.Services.AddSignalRChat();

var app = builder.Build();

app.UseApiDocumentation();

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.UseCors(SignalRExtensions.ChatCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapQuartzDashboard();

app.Run();