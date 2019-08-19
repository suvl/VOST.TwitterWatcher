using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace VOST.TwitterWatcher
{
    /// <summary>
    /// Base starting point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">the program's arguments.</param>
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config")
                .GetCurrentClassLogger();
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                logger.Error(e, "Stopping program execution");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// The ASP.NET web host builder
        /// </summary>
        /// <param name="args">the program's arguments</param>
        /// <returns>An <see cref="IWebHostBuilder" />.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(l =>
                {
                    l.ClearProviders();
                    l.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();
    }
}
