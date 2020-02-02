using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleNotes.Api.Data;
using System;

namespace SimpleNotes.Api {
    public class Program {
        public static void Main(string[] args) {
            IHost host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                try {
                    var context = services.GetRequiredService<ApplicationContext>();
                    DbInitializer.Init(context);
                } catch (Exception ex) {
                    Console.WriteLine($"Error initializing the database: {ex.Message}");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
        }

    }
}
