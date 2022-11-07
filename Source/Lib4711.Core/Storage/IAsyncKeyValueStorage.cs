namespace Lib4711.Core.Storage
{
    /// <summary>
    /// Represents a simple key-value storage.
    /// </summary>
    public interface IAsyncKeyValueStorage
    {
        /// <summary>
        /// Stores the value with given <paramref name="key"/> to the storage. An existing value with the same <paramref name="key"/> will be overwritten, if not specifically requested otherwise by <paramref name="overwriteExisting"/>.
        /// </summary>
        /// <returns>True if the value was successfully saved, False if <paramref name="overwriteExisting"/> is set to false and the key already exists.</returns>
        Task<bool> SaveValueAsync(string key, object value, bool overwriteExisting = true);

        /// <summary>
        /// Attempts to retrieve the value with the given key from the storage.
        /// </summary>
        /// <returns>A successful result with the obtained value attached; else an unsuccessful result without value.</returns>
        Task<TValue?> TryGetValueAsync<TValue>(string key);

        /// <summary>
        /// Deletes the value with the given <paramref name="key"/>.
        /// </summary>
        /// <returns>True, if the value was successfully deleted, False if it was not existing anyway.</returns>
        Task DeleteValueAsync(string key);

        /// <summary>
        /// Checks for the existance of an object with the given <paramref name="key"/>.
        /// </summary>
        /// <returns>True if an object with the <paramref name="key"/> exists, False if not.</returns>
        Task<bool> ValueExistsAsync(string key);
    }
}
