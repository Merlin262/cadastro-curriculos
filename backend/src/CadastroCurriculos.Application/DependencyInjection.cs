using System.Reflection;
using CadastroCurriculos.Application.Common.Behaviors;
using FluentValidation;
using LiteMediator.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCurriculos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddLiteMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
