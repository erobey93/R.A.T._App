using System;
using System.Collections.Concurrent;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Interface for the R.A.T. App's caching service.
    /// Defines methods for storing and retrieving cached data.
    /// </summary>
    public interface ICacheService
    {
        T Get<T>(string key) where T : class;
        void Set<T>(string key, T value, TimeSpan? expirationTime = null) where T : class;
        void Remove(string key);
    }

    /// <summary>
    /// Thread-safe in-memory caching service for the R.A.T. App.
    /// Provides temporary storage for frequently accessed data to improve performance.
    /// 
    /// Key Features:
    /// - Thread-safe operations using ConcurrentDictionary
    /// - Automatic expiration of cached items
    /// - Generic type support
    /// - Configurable cache duration
    /// 
    /// Usage:
    /// - Cache frequently accessed, rarely changed data
    /// - Store computed results temporarily
    /// - Reduce database load for common queries
    /// 
    /// Default Settings:
    /// - 30-minute default expiration time
    /// - Items automatically removed on expiration
    /// 
    /// Known Limitations:
    /// - In-memory only (no persistence)
    /// - No cache size limits
    /// - No cache eviction policies
    /// - No event notifications
    /// 
    /// Future Enhancements:
    /// - Add cache size limits
    /// - Implement eviction policies
    /// - Add cache statistics
    /// - Add event notifications
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache;

        public CacheService()
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
        }

        /// <summary>
        /// Retrieves a cached item by key.
        /// 
        /// Process:
        /// 1. Checks if key exists
        /// 2. Verifies item hasn't expired
        /// 3. Removes expired items automatically
        /// 
        /// Note: Returns null if item:
        /// - Doesn't exist
        /// - Has expired
        /// - Can't be cast to requested type
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key to lookup</param>
        /// <returns>Cached item or null if not found/expired</returns>
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

        /// <summary>
        /// Stores an item in the cache.
        /// 
        /// Process:
        /// 1. Creates cache item with value and expiration
        /// 2. Adds or updates item in thread-safe manner
        /// 
        /// Default Expiration:
        /// - 30 minutes if not specified
        /// - Custom duration via expirationTime parameter
        /// 
        /// Thread Safety:
        /// - Uses ConcurrentDictionary.AddOrUpdate
        /// - Safe for concurrent access
        /// </summary>
        /// <typeparam name="T">Type of item to cache</typeparam>
        /// <param name="key">Cache key for later retrieval</param>
        /// <param name="value">Item to cache</param>
        /// <param name="expirationTime">Optional custom expiration</param>
        public void Set<T>(string key, T value, TimeSpan? expirationTime = null) where T : class
        {
            var item = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(expirationTime ?? TimeSpan.FromMinutes(30))
            };

            _cache.AddOrUpdate(key, item, (_, _) => item);
        }

        /// <summary>
        /// Removes an item from the cache.
        /// 
        /// Thread Safety:
        /// - Uses ConcurrentDictionary.TryRemove
        /// - Safe for concurrent access
        /// 
        /// Note: Silently continues if key doesn't exist
        /// </summary>
        /// <param name="key">Cache key to remove</param>
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
