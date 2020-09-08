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
    public class AddRoleCommand : ICommand
    {
        private readonly BotDbContext _context;

        public AddRoleCommand(BotDbContext context)
        {
            _context = context;
        }

        public string Name => "AddRole";
        public IEnumerable<string> Aliases => new List<string>() { "Join" };
        public string Help => "Adds a role. Use !addrole rolename";
        public bool Public => true;
        public async Task Invoke(SocketUserMessage msg)
        {
            var requestableRoles = await Queryable.Where(_context.ManagedRoles,
                x => x.GuildId == ((SocketGuildChannel)msg.Channel).Guild.Id).ToListAsync();
            var guild = ((SocketGuildChannel)msg.Channel).Guild;
            var nameDict = guild.Roles.ToDictionary(x => x.Id, y => y.Name);
            var names = requestableRoles.ToDictionary(key => nameDict[key.RoleId].ToLower(), value => guild.Roles.SingleOrDefault(x => x.Id == value.RoleId));

            var args = msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(x => x.ToLower());

            foreach (var arg in args)
            {
                if (names.ContainsKey(arg))
                {
                    var user = (SocketGuildUser)msg.Author;
                    var role = names[arg];
                    if (role != null) await user.AddRoleAsync(role);

                }
            }
        }
    }
}
