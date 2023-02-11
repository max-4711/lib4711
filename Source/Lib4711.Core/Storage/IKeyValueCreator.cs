namespace Lib4711.Core.Storage
{
    /// <summary>
    /// Provides functionality to obtain an <see cref="IAsyncKeyValueStorage"/>.
    /// </summary>
    public interface IKeyValueStorageCreator
    {
        /// <summary>
        /// Gets (or creates, if it's not already existing) the unnamed/default storage of the application.
        /// </summary>
        IAsyncKeyValueStorage GetOrBuildDefaultKeyValueStorage();

        /// <summary>
        /// Gets (or creates, if it's not already existing) the storage with the given <paramref name="storageName"/>.
        /// </summary>
        /// <param name="storageName">Can be arbitrary chosen, will only be used to uniquely identify the storage.</param>
        IAsyncKeyValueStorage GetOrBuildNamedKeyValueStorage(string storageName);
    }
}
