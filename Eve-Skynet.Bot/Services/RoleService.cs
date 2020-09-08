using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Models.Roles;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Services
{
    public class RoleService
    {
        private readonly BotDbContext _context;
        private readonly DiscordSocketClient _client;
        private readonly BotSettings _botSettings;

        public RoleService(BotDbContext context, DiscordSocketClient client, BotSettings botSettings)
        {
            _context = context;
            _client = client;
            _botSettings = botSettings;
        }

        public async Task AddAdmin(SocketRole role)
        {
            var adminRole = new AdminRole
            {
                GuildId = role.Guild.Id,
                RoleId = role.Id,
                RoleName = role.Name
            };
            await _context.AdminRoles.AddAsync(adminRole);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserIsAdmin(SocketUserMessage msg)
        {
            if (_botSettings.SuperUserId.HasValue && _botSettings.SuperUserId.Value == msg.Author.Id) return true;
            if (msg.Author is SocketGuildUser guildUser)
            {
                var guild = guildUser.Guild.Id;
                var roles = guildUser.Roles;

                var adminRoles = await Queryable.Where(_context.AdminRoles, x => x.GuildId == guild).ToListAsync();

                foreach (var adminRole in adminRoles)
                {
                    if (roles.Any(x => x.Id == adminRole.RoleId)) return true;
                }

                return false;
            }

            return false;
        }
    }
}
