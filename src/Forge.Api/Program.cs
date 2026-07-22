using Forge.Application.Extensions;
using Forge.Api.Middleware;
using Forge.Infrastructure.Extensions;
using Forge.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

const string ForgeMobileWebDevelopmentCorsPolicy = "ForgeMobileWebDevelopment";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        ForgeMobileWebDevelopmentCorsPolicy,
        policy => policy
            .WithOrigins(
                "http://localhost:8082",
                "http://localhost:5173",
                "http://localhost:5174")
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.Configure<AdminImageStorageOptions>(builder.Configuration.GetSection("AdminImageStorage"));
builder.Services.PostConfigure<AdminImageStorageOptions>(options =>
{
    var webRoot = builder.Environment.WebRootPath
        ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    options.RootPath = string.IsNullOrWhiteSpace(options.RootPath)
        ? Path.Combine(webRoot, "uploads", "backoffice")
        : options.RootPath;
    options.PublicBasePath = string.IsNullOrWhiteSpace(options.PublicBasePath)
        ? "/uploads/backoffice"
        : options.PublicBasePath;
});

var app = builder.Build();

await app.Services.SeedMuscleGroupsAsync();
await app.Services.SeedExercisesAsync();
await app.Services.SeedAchievementsAsync();
await app.Services.SeedRaritiesAsync();
await app.Services.SeedLevelDefinitionsAsync();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();
app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var uploadsRoot = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

if (app.Environment.IsDevelopment())
{
    app.UseCors(ForgeMobileWebDevelopmentCorsPolicy);
}

app.MapControllers();

app.Run();
