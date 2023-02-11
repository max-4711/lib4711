using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Lib4711.Core.Storage;

namespace Lib4711.Blazor.Storage
{
    internal class BrowserLocalStorageKeyValueStorage : IAsyncKeyValueStorage
    {
        private readonly ProtectedBrowserStorage browserLocalStorage;
        private readonly string storageName;

        public BrowserLocalStorageKeyValueStorage(ProtectedBrowserStorage browserLocalStorage, string storageName)
        {
            this.browserLocalStorage = browserLocalStorage;
            this.storageName = storageName;
        }

        public async Task DeleteValueAsync(string key)
        {
            await this.browserLocalStorage.DeleteAsync($"{this.storageName}_{key}");
        }

        public async Task<bool> SaveValueAsync(string key, object value, bool overwriteExisting = true)
        {
            if (!overwriteExisting && await this.ValueExistsAsync(key))
            {
                return false;
            }

            await this.browserLocalStorage.SetAsync($"{this.storageName}_{key}", value);
            return true;
        }

        public async Task<TValue?> TryGetValueAsync<TValue>(string key)
        {
            var storageResult = await this.browserLocalStorage.GetAsync<TValue>($"{this.storageName}_{key}");
            if (storageResult.Success)
            {
                return storageResult.Value;
            }

            return default;
        }

        public async Task<bool> ValueExistsAsync(string key)
        {
            var storageResult = await this.browserLocalStorage.GetAsync<object>($"{this.storageName}_{key}");
            if (storageResult.Success && storageResult.Value != null)
            {
                return true;
            }

            return false;
        }
    }
}
