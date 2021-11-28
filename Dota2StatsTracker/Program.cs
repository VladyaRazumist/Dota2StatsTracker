using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Dota2StatsTracker.Config;
using Dota2StatsTracker.Enums;
using Dota2StatsTracker.Jobs;
using Dota2StatsTracker.Services;
using Dota2StatsTracker.Services.ApiServices;
using Dota2StatsTracker.Services.DbServices;
using Dota2StatsTracker.Services.HelperServices;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Dota2StatsTracker
{
    public class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["Settings:DiscordToken"];
                })
                .UseCommandService((context, config) =>
                {
                    new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose
                    };
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddDbContext<BotDbContext>()
                        .AddHostedService<CommandHandler>()
                        .AddScoped<OpenDotaApiService>()
                        .AddScoped<VkApiService>()
                        .AddScoped<UserService>()
                        .AddScoped<DotaAccountService>()
                        .AddScoped<DotaMatchService>()
                        .AddScoped<OpenDotaEmberBuilder>()
                        .AddScoped<GetNotificationsJob>()
                        .AddSingleton<DotaConstantService>()
                        .AddScoped<UserRepository>()
                        .AddScoped<DotaMatchRepository>()
                        .AddScoped<DotaAccountRepository>()
                        .AddScoped<DbUnitOfWork>()
                        .AddLavaNode(x =>
                        {
                            x.SelfDeaf = false;
                        })
                        .AddHangfire(configuration => configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseSqlServerStorage("data source=.\\MSSQLSERVER02;Initial Catalog=Hangfire;Integrated Security=True;", new SqlServerStorageOptions
                            {
                                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                QueuePollInterval = TimeSpan.Zero,
                                UseRecommendedIsolationLevel = true,
                                DisableGlobalLocks = true
                            }))
                        .AddHangfireServer()
                        .Configure<Settings>(context.Configuration.GetSection("Settings"));
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}