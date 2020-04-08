using System;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Web.Api.Infrastructure.Database;
using Web.Api.Infrastructure.Guards;

namespace Web.Api.Infrastructure.Repositories
{

    public class RepositoryFacade: IRepositoryFacade
    {
        private IDistributedCache _cache;

        private IConfiguration _configuration;

        public RepositoryFacade(IDistributedCache cache,IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
        }

        public string GetCache(string key)
        {
            try
            {
                var s = _configuration["Redis:sliding_expiration"];
                return _cache.GetString(key);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void UpdateCache(string key)
        {
            _cache.Refresh(key);
        }

        public void SetCache(string key, string message, DistributedCacheEntryOptions options)
        {
            _cache.SetString(key, message, new DistributedCacheEntryOptions()
           .SetSlidingExpiration(TimeSpan.FromSeconds(Convert.ToInt32(_configuration["Redis:sliding_expiration"])))
           .SetAbsoluteExpiration(DateTime.Now.AddHours(Convert.ToInt32(_configuration["Redis:absolute_expiration"]))));

        }
    }
}

