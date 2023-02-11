using Lib4711.Blazor.Storage;
using Lib4711.Core.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Lib4711.Blazor
{
    public static class Setup
    {
        public static IServiceCollection SetupLib4711Blazor(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKeyValueStorageCreator, BrowserLocalStorageKeyValueStorageCreator>();

            return serviceCollection;
        }
    }
}
