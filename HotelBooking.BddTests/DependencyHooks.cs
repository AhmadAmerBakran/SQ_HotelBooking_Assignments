using Microsoft.Extensions.DependencyInjection;
using HotelBooking.BddTests.Support;
using Reqnroll;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.BddTests;

[Binding]
public sealed class DependencyHooks
{
    [ScenarioDependencies]
    public static IServiceCollection RegisterServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<CustomWebApplicationFactory>();
        return services;
    }
}
