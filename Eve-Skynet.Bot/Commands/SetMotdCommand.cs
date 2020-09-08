using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Extensions;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Eve_Skynet.Bot.Commands
{
    [DiscordAuthorization]
    public class SetMotdCommand : ICommand
    {
        private readonly IList<ContinuedCommand> _continuedCommands;
        private readonly DiscordGuildMessageContext _msgContext;

        public SetMotdCommand(IList<ContinuedCommand> continuedCommands, DiscordGuildMessageContext msgContext)
        {
            _continuedCommands = continuedCommands;
            _msgContext = msgContext;
        }

        public string Name => "SetMotd";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "Sets the server welcome message";
        public bool Public => false;
        public async Task Invoke(SocketUserMessage msg)
        {
            try
            {
                var f = new Func<IServiceProvider, Task>(async (provider) =>
                {
                    try
                    {
                        var msgContext = provider.GetRequiredService<DiscordGuildMessageContext>();
                        var repo = provider.GetRequiredService<IRepository<WelcomeMessage>>();

                        await repo.AddOrUpdateAsync(new WelcomeMessage()
                        {
                            GuildId = msgContext.Guild.Id,
                            Message = msgContext.Message.Content.Trim()
                        });
                        await repo.CommitAsync();
                        var obj = await repo.Get().SingleAsync(x => x.GuildId == _msgContext.Guild.Id);
                        await msgContext.Message.Channel.SendMessageAsync("New MOTD is set to:");
                        await msgContext.Message.Channel.SendMessageAsync(obj.Message);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });

                var continuedCommand = new ContinuedCommand(f, _msgContext);
                _continuedCommands.Add(continuedCommand);

                await msg.Channel.SendMessageAsync("Type in a message to do stuff");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
