using Discord.WebSocket;

namespace Eve_Skynet.Bot
{
    public static class BotResponses
    {
        public static string Error => "An error occurred";
        public static string AccessDenied(SocketGuildUser user) => $"I'm sorry {user.Mention}, i'm afraid i can't do that";
    }
}
