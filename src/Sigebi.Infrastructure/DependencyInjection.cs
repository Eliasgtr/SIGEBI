using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sigebi.Application.Abstractions;
using Sigebi.Infrastructure.Persistence;

namespace Sigebi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? "Data Source=sigebi.db";

        services.AddDbContext<SigebiDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ILibraryDataAccess, LibraryDataAccess>();
        return services;
    }
}
