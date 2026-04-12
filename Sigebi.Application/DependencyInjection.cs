using Microsoft.Extensions.DependencyInjection;
using Sigebi.Application.Services;

namespace Sigebi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILibraryApplicationService, LibraryApplicationService>();
        return services;
    }
}
