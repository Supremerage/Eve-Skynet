using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Eve_Skynet.Bot.Interfaces
{
    public interface IContinuedCommand
    {
        string Name { get; }
        IEnumerable<string> Aliases { get; }
        string Help { get; }
        bool Public { get; }
        DateTime CreatedAt { get; }

        Task Next(SocketUserMessage msg);
        //Task Invoke(SocketUserMessage msg);

    }
}
