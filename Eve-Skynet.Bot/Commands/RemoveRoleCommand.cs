using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Commands
{
    public class RemoveRoleCommand : ICommand
    {
        private readonly BotDbContext _context;

        public RemoveRoleCommand(BotDbContext context)
        {
            _context = context;
        }

        public string Name => "RemoveRole";
        public IEnumerable<string> Aliases => new List<string>() { "Leave" };
        public string Help => "Leaves a group";
        public bool Public => true;
        public async Task Invoke(SocketUserMessage msg)
        {
            var requestableRoles = await Queryable.Where(_context.ManagedRoles,
                x => x.GuildId == ((SocketGuildChannel)msg.Channel).Guild.Id).ToListAsync();
            var guild = ((SocketGuildChannel)msg.Channel).Guild;
            var nameDict = guild.Roles.ToDictionary(x => x.Id, y => y.Name);
            var names = requestableRoles.ToDictionary(key => nameDict[key.RoleId].ToLower(), value => guild.Roles.SingleOrDefault(x => x.Id == value.RoleId));
            var user = (SocketGuildUser)msg.Author;
            var args = msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(x => x.ToLower());

            foreach (var arg in args)
            {
                if (names.ContainsKey(arg) && user.Roles.Any(x => x.Name.ToLower() == arg))
                {
                    var role = names[arg];
                    if (role != null) await user.RemoveRoleAsync(role);
                }
            }

        }
    }
}
