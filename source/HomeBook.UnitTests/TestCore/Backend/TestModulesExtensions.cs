using HomeBook.Backend.Core.Search;
using HomeBook.Backend.Factories;
using HomeBook.Backend.ModuleCore;
using HomeBook.Backend.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.UnitTests.TestCore.Backend;

public static class TestModulesExtensions
{
    public static IServiceCollection AddBackendModulesForTestEnvironment(this IServiceCollection sc,
        IConfiguration c)
    {
        HomeBookOptions hb = new();
        ModuleBuilder moduleBuilder = new(hb, sc, c);

        moduleBuilder
            .AddModule<HomeBook.Backend.Module.Finances.Module>()
            .AddModule<HomeBook.Backend.Module.Kitchen.Module>();

        return sc;
    }
}
