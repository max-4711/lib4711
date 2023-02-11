using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Moq;
using Lib4711.Blazor.Storage;

namespace Lib4711.Tests
{
    [TestClass]
    public class BrowserLocalKeyValueStorageShould
    {
        private const string SAMPLE_STORAGE_NAME = "sampleStorage";
        private const string SAMPLE_KEY = "sampleKey";

        private readonly string STORAGE_AND_KEY_NAME = $"{SAMPLE_STORAGE_NAME}_{SAMPLE_KEY}";

        private Mock<ProtectedBrowserStorage>? browserStorageMock;

        [TestInitialize]
        public void TestSetup()
        {
            this.browserStorageMock = new Mock<ProtectedBrowserStorage>();
        }

        [Ignore("Unable to mock ProtectedBrowserStorage")]
        [TestMethod]
        public async Task RequestDeletionWithStorageAndKeyName()
        {
            var testObject = new BrowserLocalStorageKeyValueStorage(this.browserStorageMock!.Object, SAMPLE_STORAGE_NAME);

            await testObject.DeleteValueAsync(SAMPLE_KEY);

            this.browserStorageMock.Verify(x => x.DeleteAsync(STORAGE_AND_KEY_NAME), Times.Once);
        }
    }
}