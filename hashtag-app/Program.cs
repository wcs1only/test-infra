// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.Tests.HashTagApp
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Dapr.Actors.AspNetCore;
    using Dapr.Tests.HashTagApp.Actors;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static string GetEnvironment() => 
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "hashtag-actor.dev";

        public static IHostBuilder CreateHostBuilder(string[] args) {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, config) =>
                {
                    config.ClearProviders();
                    config.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var appSettings = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true, reloadOnChange: true)
                        .AddCommandLine(args)
                        .Build();

                    var host = webBuilder.UseStartup<Startup>()
                        .UseUrls(urls: $"http://*:{appSettings[AppSettings.DaprHTTPAppPort]}");

                    switch (appSettings[AppSettings.AppType]) {
                        case AppSettings.HashTagActor:
                            host.UseActors(actorRuntime => actorRuntime.RegisterActor<HashTagActor>());
                            break;
                    }

                    Console.WriteLine($"Starting HashTag App as {appSettings[AppSettings.AppType]} role...");
                });

            return hostBuilder;
        }
    }
}
