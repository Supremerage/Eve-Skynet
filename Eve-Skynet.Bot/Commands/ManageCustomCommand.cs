using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eve_Skynet.Bot.Commands
{
    [DiscordAuthorization]
    public class ManageCustomCommand : ICommand
    {
        private readonly IRepository<CustomCommand> _customCommandRepository;
        private readonly IList<ContinuedCommand> _continuedCommands;
        private readonly DiscordGuildMessageContext _msgContext;
        private readonly char _prefix;

        public ManageCustomCommand(IConfiguration configuration, IRepository<CustomCommand> customCommandRepository, IList<ContinuedCommand> continuedCommands, DiscordGuildMessageContext msgContext)
        {
            _customCommandRepository = customCommandRepository;
            _continuedCommands = continuedCommands;
            _msgContext = msgContext;
            _prefix = configuration["CommandPrefix"].Single();
        }
        public string Name => "ManageCustomCommand";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "";
        public bool Public => false;
        public async Task Invoke(SocketUserMessage msg)
        {
            var content = msg.Content.Trim();
            var parameters = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var modifier = parameters[1].ToLower();
            var guild = ((SocketGuildChannel)msg.Channel).Guild;

            switch (modifier)
            {
                case "create":
                    var name = parameters[2].ToString();

                    var command = new Func<IServiceProvider, Task>(async (services) =>
                    {
                        var repo = services.GetRequiredService<IRepository<CustomCommand>>();
                        var msgContext = services.GetRequiredService<DiscordGuildMessageContext>();

                        var cmd = new CustomCommand()
                        {
                            Response = msgContext.Message.Content,
                            GuildId = msgContext.Guild.Id,
                            Name = name
                        };
                        await repo.AddOrUpdateAsync(cmd);
                        await repo.CommitAsync();
                    });

                    var continuedCommand = new ContinuedCommand(command, _msgContext);

                    _continuedCommands.Add(continuedCommand);
                    await msg.Channel.SendMessageAsync("Enter command response");
                    break;

                case "remove":
                    var removedName = parameters[2].Trim().ToLower();
                    var removedCmd = await _customCommandRepository.Get().SingleOrDefaultAsync(x =>
                        x.GuildId == _msgContext.Guild.Id && x.Name.ToLower() == removedName);

                    if (removedCmd != null)
                    {
                        _customCommandRepository.Remove(removedCmd);
                        await _customCommandRepository.CommitAsync();
                        await msg.Channel.SendMessageAsync($"Deleted {removedCmd.Name}");
                    }
                    else
                    {
                        await msg.Channel.SendMessageAsync("Custom command not found");
                    }
                    break;

                case "list":
                    var commands = await _customCommandRepository.Get().Where(x => x.GuildId == guild.Id).ToListAsync();
                    if (!commands.Any())
                    {
                        await msg.Channel.SendMessageAsync("No commands registered.");
                        break;
                    }

                    var commandResponse = "Custom Commands:\n";
                    foreach (var cmd in commands) commandResponse += (_prefix + cmd.Name + "\n");
                    commandResponse = commandResponse.Trim('\n');
                    await msg.Channel.SendMessageAsync(commandResponse);
                    break;

                default:
                    throw new ArgumentException("Must contain valid modifier");
            }


        }
    }
}
