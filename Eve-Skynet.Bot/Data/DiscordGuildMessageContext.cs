using Discord.WebSocket;

namespace Eve_Skynet.Bot.Data
{
    public class DiscordGuildMessageContext
    {
        public SocketGuildUser Author { get; private set; }
        public SocketGuildChannel Channel { get; private set; }
        public SocketGuild Guild { get; private set; }
        public SocketUserMessage Message { get; private set; }

        public void Build(SocketGuildUser author, SocketGuildChannel channel, SocketGuild guild, SocketUserMessage message)
        {
            Author = author;
            Channel = channel;
            Guild = guild;
            Message = message;
        }
    }
}
