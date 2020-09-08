using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;

namespace Eve_Skynet.Bot.Models
{
    public class ContinuedCommand
    {
        public ContinuedCommand(Func<IServiceProvider, Task> command, DiscordGuildMessageContext context)
        {
            Command = command;
            CreatedAt = DateTime.UtcNow;
            User = context.Author;
            Channel = context.Channel;
            Guild = context.Guild;
        }


        public DateTime CreatedAt { get; }
        public SocketGuildUser User { get; }
        public SocketGuildChannel Channel { get; }
        public SocketGuild Guild { get; }
        public Func<IServiceProvider, Task> Command { get; }

        public bool Match(DiscordGuildMessageContext context)
        {
            return context.Author.Id == User.Id && context.Channel.Id == Channel.Id &&
                   context.Guild.Id == Guild.Id;
        }

    }
}
