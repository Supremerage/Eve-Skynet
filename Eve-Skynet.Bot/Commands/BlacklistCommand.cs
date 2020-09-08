using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Services;

namespace Eve_Skynet.Bot.Commands
{
    [DiscordAuthorization]
    public class BlacklistCommand : ICommand
    {
        private readonly RoleService _roleService;
        private readonly BotDbContext _context;

        public BlacklistCommand(RoleService roleService, BotDbContext context)
        {
            _roleService = roleService;
            _context = context;
        }

        public string Name => "Blacklist";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "Stops the bot from running commands in the mentioned channels";
        public bool Public => false;

        public async Task Invoke(SocketUserMessage msg)
        {
            var blacklisted = new List<string>();
                if (msg.MentionedChannels.Count > 0)
                {
                    foreach (var channel in msg.MentionedChannels)
                    {
                        if (!await _context.BlacklistedChannels.AnyAsync(x =>
                            x.GuildId == channel.Guild.Id && x.ChannelId == channel.Id))
                        {
                            var blacklistedChannel = new BlacklistedChannel
                            {
                                GuildId = channel.Guild.Id,
                                ChannelId = channel.Id
                            };
                            await _context.BlacklistedChannels.AddAsync(blacklistedChannel);
                        blacklisted.Add(channel.Name);
                        }
                    }

                    await _context.SaveChangesAsync();

                    var successResponse =
                        $"Added channels: {string.Join(", ", blacklisted)} to blacklist";
                    await msg.Channel.SendMessageAsync(successResponse);
                }
                else
                {
                    await msg.Channel.SendMessageAsync("Please mention one or more channels");
                }
        }
    }
}
