using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eve_Skynet.Bot.Services
{
    public class CmdService
    {
        private readonly BotDbContext _context;
        private readonly CommandCache _commandCache;
        private readonly RoleService _roleService;
        private readonly ILogger<CmdService> _logger;
        private readonly DiscordGuildMessageContext _msgContext;
        private readonly IRepository<CustomCommand> _customCommandRepository;
        private readonly ICollection<ICommand> _commands;
        private readonly char _prefix;

        public CmdService(IConfiguration configuration, BotDbContext context, CommandCache commandCache, RoleService roleService, ILogger<CmdService> logger, IServiceProvider serviceProvider, DiscordGuildMessageContext msgContext, IRepository<CustomCommand> customCommandRepository)
        {
            _context = context;
            _commandCache = commandCache;
            _roleService = roleService;
            _logger = logger;
            _msgContext = msgContext;
            _customCommandRepository = customCommandRepository;
            _prefix = configuration["CommandPrefix"].Single();
            var commands = new List<ICommand>();

            var commandTypes = _commandCache.GetTypes();

            foreach (var type in commandTypes)
            {
                var service = (ICommand)serviceProvider.GetRequiredService(type);
                commands.Add(service);
            }

            _commands = commands;
        }

        public async Task RunCommand(SocketMessage msg)
        {
            try
            {
                var content = msg.Content.Trim().ToLower();

                if (msg is SocketUserMessage userMsg &&
                    userMsg.Channel is SocketGuildChannel guildChannel && userMsg.Author is SocketGuildUser author && content.StartsWith(_prefix))
                {

                    var isBlacklisted = await _context.BlacklistedChannels.AnyAsync(x =>
                        x.GuildId == guildChannel.Guild.Id && x.ChannelId == guildChannel.Id);

                    if (!isBlacklisted)
                    {
                        var cmdTxt = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
                        _logger.LogInformation($"User \"{_msgContext.Author.Username}#{_msgContext.Author.DiscriminatorValue}\" is executing command \"{cmdTxt}\" on guild \"{_msgContext.Guild.Name}\"");


                        const StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;

                        var command = _commands.SingleOrDefault(x =>
                            cmdTxt.Equals(_prefix + x.Name, ignoreCase) ||
                            x.Aliases.Any(y => cmdTxt.Equals(_prefix + y, ignoreCase)));

                        var customCommand = _customCommandRepository.Get().Where(x => x.GuildId == _msgContext.Guild.Id).ToList()
                            .SingleOrDefault(x => cmdTxt.Equals(_prefix + x.Name, ignoreCase));
                        if (command != null)
                        {
                            //Check if command needs authorization
                            var commandRequiresAuthentication =
                                _commandCache.GetAuthorizedTypes().Any(x => x.IsInstanceOfType(command));
                            if (commandRequiresAuthentication)
                            {
                                await msg.Channel.SendMessageAsync("This is a spooky command");
                                if (await _roleService.UserIsAdmin(userMsg)) await command.Invoke(userMsg);
                                else await msg.Channel.SendMessageAsync(BotResponses.AccessDenied(author));
                            }
                            else
                            {
                                await command.Invoke(userMsg);
                            }
                        }
                        else if (customCommand != null)
                        {
                            await msg.Channel.SendMessageAsync(customCommand.Response);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid Command: " + cmdTxt);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command");
                switch (ex)
                {
                    case ArgumentException _:
                        _logger.LogWarning(ex.Message);
                        break;
                    default:
                        await msg.Channel.SendMessageAsync(BotResponses.Error);
                        break;
                }
            }
        }



    }
}
