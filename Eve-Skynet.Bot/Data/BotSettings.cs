using Microsoft.Extensions.Configuration;

namespace Eve_Skynet.Bot.Data
{
    public class BotSettings
    {
        public BotSettings(IConfiguration configuration)
        {
            var hasSuperUser =  ulong.TryParse(configuration["Discord:SuperUserId"], out ulong superUserId);
            if (hasSuperUser) SuperUserId = superUserId;
        }

        public ulong? SuperUserId { get; }
    }
}
