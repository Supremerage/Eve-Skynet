using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Eve_Skynet.Bot.Interfaces
{
    public interface ICommand
    {
        string Name { get; }
        IEnumerable<string> Aliases { get; }
        string Help { get; }
        bool Public { get; }
        Task Invoke(SocketUserMessage msg);

    }
}
