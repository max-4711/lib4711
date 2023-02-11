using Lib4711.Core.Storage;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Lib4711.Blazor.Storage
{
    internal class BrowserLocalStorageKeyValueStorageCreator : IKeyValueStorageCreator
    {
        private const string STORAGE_DEFAULT_NAME = "lib4711DefaultStorage";

        private readonly Dictionary<string, IAsyncKeyValueStorage> storages = new Dictionary<string, IAsyncKeyValueStorage>();

        private readonly ProtectedBrowserStorage browserLocalStorage;

        public BrowserLocalStorageKeyValueStorageCreator(ProtectedBrowserStorage browserLocalStorage)
        {
            this.browserLocalStorage = browserLocalStorage;
        }

        public IAsyncKeyValueStorage GetOrBuildDefaultKeyValueStorage()
        {
            return this.GetOrBuildNamedKeyValueStorage(STORAGE_DEFAULT_NAME);
        }

        public IAsyncKeyValueStorage GetOrBuildNamedKeyValueStorage(string storageName)
        {
            if (storages.TryGetValue(storageName, out IAsyncKeyValueStorage? storage) && storage != null)
            {
                return storage;
            }

            var newStorage = new BrowserLocalStorageKeyValueStorage(browserLocalStorage, storageName);
            storages[storageName] = newStorage;

            return newStorage;
        }
    }
}
