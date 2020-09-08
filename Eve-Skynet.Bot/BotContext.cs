using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eve_Skynet.Bot
{
    public class BotContext : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly BotSettings _settings;
        private readonly ILogger<BotContext> _logger;
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;

        public BotContext(DiscordSocketClient client, IConfiguration configuration, IHostApplicationLifetime appLifetime, BotSettings settings, ILogger<BotContext> logger, IServiceProvider provider)
        {
            _configuration = configuration;
            _appLifetime = appLifetime;
            _settings = settings;
            _logger = logger;
            _provider = provider;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a Db scope for migrations
            using (var scope = _provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

                _logger.LogInformation("Bot starting...");
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pendingMigrations.Any())
                {
                    _logger.LogWarning("Database out of date. Applying migrations...");
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Migrations applied.");
                }
            }
            if (_settings.SuperUserId.HasValue)
            {
                _logger.LogWarning($"Super User is set and registered as ID: {_settings.SuperUserId.Value}. This user will have admin access on all guilds.");
            }

            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);


            var token = _configuration["Discord:Token"];
            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            _client.UserJoined += async (user) =>
            {
                using (var scope = _provider.CreateScope())
                {


                }
            };

            _client.Disconnected += (ex) =>
            {
                _logger.LogWarning("Disconnected from Discord. Attempting to reconnect.");

                return Task.CompletedTask;
            };
            _client.Connected += () =>
            {
                _logger.LogInformation("Successfully connected to Discord.");
                return Task.CompletedTask;
            };

            _client.Ready += () =>
            {
                _logger.LogInformation($"Bot is connected as {_client.CurrentUser.Username}!");

                return Task.CompletedTask;
            };

            _client.MessageReceived += async (msg) =>
            {
                using (var scope = _provider.CreateScope())
                {
                    _logger.LogDebug(msg.Author.Username + ": " + msg.Content);
                    if (msg is SocketUserMessage userMsg && msg.Author is SocketGuildUser guildUser && msg.Channel is SocketGuildChannel guildChannel && !guildUser.IsBot && !guildUser.IsWebhook)
                    {
                        var msgContext = scope.ServiceProvider.GetRequiredService<DiscordGuildMessageContext>();
                        msgContext.Build(guildUser, guildChannel, guildChannel.Guild, userMsg);

                        var continuedCommands = scope.ServiceProvider.GetService<IList<ContinuedCommand>>();
                        var continuedCmd = continuedCommands.SingleOrDefault(x => x.Match(msgContext));
                        if (continuedCmd != null)
                        {
                            continuedCommands.Remove(continuedCmd);
                            await continuedCmd.Command.Invoke(scope.ServiceProvider);
                        }
                        else
                        {
                            var cmdService = scope.ServiceProvider.GetRequiredService<CmdService>();

                            await cmdService.RunCommand(msg).ConfigureAwait(true);
                        }
                    }
                }
            };
        }

        private void OnStopped()
        {
            _logger.LogInformation("Bot stopped");
        }

        private void OnStopping()
        {
            _logger.LogInformation("Bot stopping...");
        }

        private void OnStarted()
        {
            _logger.LogInformation("Bot started!");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

            await _client.StopAsync();
            await _client.LogoutAsync();
        }
    }



}
