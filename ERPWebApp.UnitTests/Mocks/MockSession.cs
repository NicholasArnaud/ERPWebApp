using Microsoft.AspNetCore.Http;

namespace ERPWebApp.UnitTests.Mocks
{
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = [];

        public IEnumerable<string> Keys => _sessionStorage.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

        public T GetObjectFromJson<T>(string key)
        {
            return default;
        }

        public void SetObjectAsJson<T>(string key, T value)
        {
        }
    }
}