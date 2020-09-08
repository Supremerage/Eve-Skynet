using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Eve_Skynet.Bot.Data
{
    public class CommandCache
    {
        private readonly IMemoryCache _cache;

        public CommandCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Refresh()
        {
            _cache.Remove(CacheKeys.CommandTypes);
            _cache.Remove(CacheKeys.AuthorizedCommandTypes);
        }

        public IEnumerable<Type> GetTypes()
        {
            var types = _cache.GetOrCreate<IEnumerable<Type>>(CacheKeys.CommandTypes,  entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);

                var assembly = typeof(Program).Assembly;
                var cmds = assembly.GetTypes().Where(x => typeof(ICommand).IsAssignableFrom(x) && x.IsClass).ToList();

                return cmds;
            });
            return types;
        }

        public IEnumerable<Type> GetAuthorizedTypes()
        {
            var types = _cache.GetOrCreate<IEnumerable<Type>>(CacheKeys.AuthorizedCommandTypes,  entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);

                var assembly = typeof(Program).Assembly;
                var cmds = assembly.GetTypes().Where(x => typeof(ICommand).IsAssignableFrom(x) && x.GetCustomAttribute(typeof(DiscordAuthorizationAttribute)) != null && x.IsClass).ToList();

                return cmds;
            });
            return types;
        }
    }
}
