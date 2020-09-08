using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Services;
using Microsoft.EntityFrameworkCore;

namespace Eve_Skynet.Bot.Commands
{
    [DiscordAuthorization]
    public class ManageRoleCommand : ICommand
    {
        private readonly RoleService _roleService;
        private readonly BotDbContext _context;

        public ManageRoleCommand(RoleService roleService, BotDbContext context)
        {
            _roleService = roleService;
            _context = context;
        }

        public string Name => "ManageRole";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "Makes a role requestable through the bot";
        public bool Public => false;

        public async Task Invoke(SocketUserMessage msg)
        {
                var args = msg.Content.Trim().Split(' ').Skip(1).ToList();
                var rolesToManage = msg.MentionedRoles.Where(x => !x.IsEveryone && !x.IsManaged).ToList();
                var user = (SocketGuildUser)msg.Author;
                var guild = user.Guild;

                if (rolesToManage.Count > 0)
                {

                    var existingRoles = new List<SocketRole>();
                    var newRoles = new List<SocketRole>();
                    foreach (var role in msg.MentionedRoles)
                    {
                        if (await EntityFrameworkQueryableExtensions.AnyAsync(_context.ManagedRoles, x => x.GuildId == guild.Id && x.RoleId == role.Id))
                        {
                            existingRoles.Add(role);
                            continue;
                        }
                        var managedRole = new ManagedRole()
                        {
                            GuildId = guild.Id,
                            RoleId = role.Id
                        };
                        await _context.ManagedRoles.AddAsync(managedRole);
                        newRoles.Add(role);

                    }

                    await _context.SaveChangesAsync();

                    if (existingRoles.Any())
                    {
                        var existingResponse =
                            $"Already managing: {string.Join(", ", existingRoles.Select(x => x.Name))}";
                        await msg.Channel.SendMessageAsync(existingResponse);
                    }

                    var successResponse =
                        $"Now managing roles: {string.Join(", ", newRoles.Select(x => x.Name))}";
                    await msg.Channel.SendMessageAsync(successResponse);
                }
                else if (args[0].ToLower() == "create" && args.Count > 1)
                {
                    var newRoles = args.Skip(1);
                    foreach (var newRole in newRoles)
                    {
                        var role = await guild.CreateRoleAsync(newRole, null, null, false, false);

                        var managedRole = new ManagedRole()
                        {
                            GuildId = guild.Id,
                            RoleId = role.Id
                        };
                        await _context.ManagedRoles.AddAsync(managedRole);
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    await msg.Channel.SendMessageAsync("Please mention one or more roles");
                }


        }
    }
}
