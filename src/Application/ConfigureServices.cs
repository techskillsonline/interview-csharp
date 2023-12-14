using System.Reflection;
using UrlShortenerService.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Common.Services;
using HashidsNet;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        _ = services.AddSingleton<IHashGeneratorService>(provider =>
        {
            var hashids = provider.GetService<IHashids>();
            if (hashids != null)
            {
                return new HashGeneratorService(hashids, Int32.Parse(configuration["EncodeURLConfig:MinNumber"] ?? "0"), Int32.Parse(configuration["EncodeURLConfig:MaxNumber"] ?? "123456789"));
            }
            return default!;
        });

        return services;
    }
}

