using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Eve_Skynet.Bot.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly CommandCache _commandCache;
        private readonly RoleService _roleService;
        private readonly IRepository<CustomCommand> _customCommandRepository;
        private readonly DiscordGuildMessageContext _msgContext;
        private readonly char _prefix;

        public HelpCommand(CommandCache commandCache, RoleService roleService, IRepository<CustomCommand> customCommandRepository, IConfiguration configuration, DiscordGuildMessageContext msgContext)
        {
            _commandCache = commandCache;
            _roleService = roleService;
            _customCommandRepository = customCommandRepository;
            _msgContext = msgContext;
            _prefix = configuration["CommandPrefix"].Single();
        }

        public string Name => "Help";
        public IEnumerable<string> Aliases => new List<string>() { "Commands" };
        public string Help => "List commands or provides help on a specific command";
        public bool Public => true;
        public async Task Invoke(SocketUserMessage msg)
        {
            var commandTypes = _commandCache.GetTypes();
            var commands = new List<ICommand>();

            foreach (var cmdType in commandTypes)
            {
                var cmd = (ICommand)FormatterServices.GetUninitializedObject(cmdType);
                commands.Add(cmd);
            }

            var customCommands = await _customCommandRepository.Get().Where(x => x.GuildId == _msgContext.Guild.Id).ToListAsync();

            var args = msg.Content.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);


            if (args.Length > 1)
            {
                var cmdArg = args[1].Trim().ToLower();
                var selectedCmd = commands.SingleOrDefault(x =>
                    x.Name.ToLower() == cmdArg || x.Aliases.Any(y => y.ToLower() == cmdArg));

                if (selectedCmd == null)
                {
                    var customCmd = await _customCommandRepository.Get()
                        .SingleOrDefaultAsync(x => x.GuildId == _msgContext.Guild.Id);
                    if (customCmd == null)
                    {
                        await msg.Channel.SendMessageAsync("I don't know that command");
                        return;
                    }

                    await msg.Channel.SendMessageAsync("Responds with a text string");
                }

                else if (!selectedCmd.Public)
                {
                    if (await _roleService.UserIsAdmin(msg))
                    {
                        await msg.Channel.SendMessageAsync(selectedCmd.Help);
                    }
                }
                else
                {
                    await msg.Channel.SendMessageAsync(selectedCmd.Help);
                }

            }
            else
            {
                var help = "Available commands:";

                if (await _roleService.UserIsAdmin(msg))
                {
                    foreach (var command in commands)
                    {
                        help += "\n";
                        help += FormatHelpCommand(command);
                    }
                }
                else
                {
                    var cmds = commands.Where(x => x.Public).ToList();

                    foreach (var command in cmds)
                    {
                        help += "\n";
                        help += FormatHelpCommand(command);
                    }
                }

                foreach (var customCommand in customCommands)
                {
                    help += "\n";
                    help += _prefix + customCommand.Name;
                }

                help += "\n";
                help += "Type !help <command> for more.";
                await msg.Channel.SendMessageAsync(help);

            }

        }

        private string FormatHelpCommand(ICommand cmd)
        {
            var helpStr = "";
            var cmdName = $"**!{cmd.Name}**";
            helpStr += cmdName;
            if (cmd.Aliases.Any())
            {
                var aliasString = $" ({string.Join(", ", cmd.Aliases.Select(x => _prefix + x))})";
                helpStr += aliasString;
            }

            return helpStr;
        }
    }
}
