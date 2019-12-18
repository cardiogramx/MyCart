using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MyCart.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<T> SetAsync<T>(string key, T item, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        T Get<T>(string key);
        T Set<T>(string key, T item);
        void Remove(string key);

        //event handler to notify any subscriber when a cart has exceeded its stay in the redis
        event EventHandler OnExpiring;
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache distributedCache;
        private readonly int _daysLeft = 1;

        public event EventHandler OnExpiring;
        private ConnectionMultiplexer connection;


        public CacheService(IDistributedCache cache)
        {
            this.distributedCache = cache;

            connection = ConnectionMultiplexer.Connect("mycart.redis.cache.windows.net");
            ISubscriber subscriber = connection.GetSubscriber();

            //get notified when redis cache key (cartId) expires
            subscriber.Subscribe("__keyevent@0__:expired", (channel, key) =>
            {
                //raise event
                OnExpiring?.Invoke(key.ToString(), new EventArgs());
            });
        }


        public async Task<T> SetAsync<T>(string key, T item, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(item);

            var options = new DistributedCacheEntryOptions();

            options.SlidingExpiration = TimeSpan.FromMinutes(_daysLeft);

            await distributedCache.SetStringAsync(key, json, options, cancellationToken);

            return item;
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(T);
            }

            var json = await distributedCache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            var response = JsonConvert.DeserializeObject<T>(json);

            return response;
        }
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }


        public T Set<T>(string key, T item)
        {
            var json = JsonConvert.SerializeObject(item);

            var options = new DistributedCacheEntryOptions();

            options.SlidingExpiration = TimeSpan.FromMinutes(_daysLeft);

            distributedCache.SetString(key, json, options);

            return item;
        }
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(T);
            }

            var json = distributedCache.GetString(key);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            var response = JsonConvert.DeserializeObject<T>(json);

            return response;
        }
        public void Remove(string key)
        {
            distributedCache.Remove(key);
        }

    }

}
