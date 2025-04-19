using System;
using System.Collections.Concurrent;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// A simple thread-safe in-memory cache service.
    /// </summary>
    public interface ICacheService
    {
        T Get<T>(string key) where T : class;
        void Set<T>(string key, T value, TimeSpan? expirationTime = null) where T : class;
        void Remove(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache;

        public CacheService()
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
        }

        public T Get<T>(string key) where T : class
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.ExpirationTime > DateTime.UtcNow)
                {
                    return (T)item.Value;
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }
            return null;
        }

        public void Set<T>(string key, T value, TimeSpan? expirationTime = null) where T : class
        {
            var item = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(expirationTime ?? TimeSpan.FromMinutes(30))
            };

            _cache.AddOrUpdate(key, item, (_, _) => item);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
}
