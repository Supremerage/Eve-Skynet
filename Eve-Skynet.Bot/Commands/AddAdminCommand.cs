using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Services;

namespace Eve_Skynet.Bot.Commands
{
    [DiscordAuthorization]
    public class AddAdminCommand : ICommand
    {
        private readonly RoleService _roleService;

        public AddAdminCommand(RoleService roleService)
        {
            _roleService = roleService;
        }

        public string Name => "Admin";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "Adds an admin role.";
        public bool Public => false;

        public async Task Invoke(SocketUserMessage msg)
        {
            if (msg.MentionedRoles.Count > 0)
            {
                foreach (var role in msg.MentionedRoles)
                {
                    await _roleService.AddAdmin(role);
                }

                var successResponse =
                    $"Added roles: {string.Join(", ", msg.MentionedRoles.Select(x => x.Name))} as admin";
                await msg.Channel.SendMessageAsync(successResponse);
            }
            else
            {
                await msg.Channel.SendMessageAsync("Please mention one or more roles");
            }
        }
    }
}
