using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Eve_Skynet.Bot.Data;
using Eve_Skynet.Bot.Interfaces;
using Eve_Skynet.Bot.Models;
using Eve_Skynet.Bot.Models.Roles;
using Eve_Skynet.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eve_Skynet.Bot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommands(this IServiceCollection services)
        {
            var assembly = typeof(Program).Assembly;
            var cmds = assembly.GetTypes().Where(x => typeof(ICommand).IsAssignableFrom(x) && x.IsClass).ToList();

            foreach (var cmd in cmds)
            {
                services.AddScoped(cmd);
            }
        }


        public static void AddBotRequirements(this IServiceCollection services, IConfiguration configuration)
        {
            
            
            //services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddMemoryCache();

            //Initialize the bots settings
            var botSettings = new BotSettings(configuration);
            services.AddSingleton(botSettings);

            //Add repositories for database models
            services.AddScoped<IRepository<BlacklistedChannel>, Repository<BlacklistedChannel>>();
            services.AddScoped<IRepository<ManagedRole>, Repository<ManagedRole>>();
            services.AddScoped<IRepository<ManagedUserRole>, Repository<ManagedUserRole>>();
            services.AddScoped<IRepository<WelcomeMessage>, Repository<WelcomeMessage>>();
            services.AddScoped<IRepository<AdminRole>, Repository<AdminRole>>();
            services.AddScoped<IRepository<CustomCommand>, Repository<CustomCommand>>();

            //Add the type cache for our commands
            services.AddSingleton<CommandCache>();
            //Add the discord websocket client
            services.AddSingleton(new DiscordSocketClient());
            services.AddSingleton<IList<ContinuedCommand>, List<ContinuedCommand>>();

            //Register DbContext in DI and add the database from the configuration file

            services.AddDbContext<BotDbContext>(o =>
            {
                switch (configuration["DbProvider"])
                {
                    case "mssql":
                        o.UseSqlServer(configuration.GetConnectionString("Default"));
                        break;
                    case "postgres":
                        o.UseNpgsql(configuration.GetConnectionString("Default"));
                        break;
                    default: throw new Exception("No valid DbProvider found");
                }

            });
            services.AddScoped<DiscordGuildMessageContext>();

            services.AddScoped<CmdService>();
            services.AddScoped<RoleService>();
            
            //This registers all commands inheriting from the ICommand interface in the DI container
            //Allowing us to use dependency injection within the Commands
            services.AddCommands();
        }
    }
}
