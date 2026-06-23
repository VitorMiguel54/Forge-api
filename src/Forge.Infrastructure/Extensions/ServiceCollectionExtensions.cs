using Forge.Application.Interfaces;
using Forge.Infrastructure.Data;
using Forge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=Forge;Trusted_Connection=True;TrustServerCertificate=True";

        services.AddDbContext<ForgeDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IExerciseRepository, ExerciseRepository>();

        return services;
    }
}
