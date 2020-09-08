using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Commands
{
    public class ListRolesCommand : ICommand
    {
        private readonly BotDbContext _context;

        public ListRolesCommand(BotDbContext context)
        {
            _context = context;
        }

        public string Name => "ListRoles";
        public IEnumerable<string> Aliases => new List<string>() { "Roles" };
        public string Help => "Lists all requestable roles";
        public bool Public => true;
        public async Task Invoke(SocketUserMessage msg)
        {
            var requestableRoles = await Queryable.Where(_context.ManagedRoles,
                x => x.GuildId == ((SocketGuildChannel)msg.Channel).Guild.Id).ToListAsync();
            var guild = ((SocketGuildChannel)msg.Channel).Guild;


            var deletedRoles = requestableRoles.Where(x => !guild.Roles.Any(y => y.Id == x.RoleId));
            _context.ManagedRoles.RemoveRange(deletedRoles);
            await _context.SaveChangesAsync();

            var roleNames = guild.Roles.ToDictionary(x => x.Id, y => y.Name);

            var roles = requestableRoles.Select(x => roleNames[x.RoleId]);

            var response = $"{string.Join(", ", roles)}";

            await msg.Channel.SendMessageAsync(response);
        }
    }
}
