using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VOST.TwitterWatcher
{
    /// <summary>
    /// CI/CD and configurations
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup .ctor
        /// </summary>
        /// <param name="configuration">The app's configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <summary>
        /// DI configurations
        /// </summary>
        /// <param name="services">The DI container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.Configure<Core.Configuration.TwitterApiConfiguration>(
                Configuration.GetSection("TwitterApi"));
            services.Configure<Core.Configuration.MongoDbClientConfiguration>(
                Configuration.GetSection("MongoDb"));

            services.AddSingleton<Core.Interfaces.ITwitterBackgroundWatcher, Background.TwitterBackgroundWatcher>();
            services.AddHostedService<Background.TwitterBackgroundWatcherHostedService>();

            services.AddSingleton<Core.Interfaces.ITweetRepository, Repo.TweetRepository>();

            services.AddSingleton<Core.Interfaces.IKeywordRepository, Repo.KeywordRepository>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "VOST Twitter Watcher",
                    Version = "v1"
                });
            });
        }

        /// <summary>
        /// The DI and pipeline execution.
        /// </summary>
        /// <param name="app">The aspnet core application.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VOST Twitter Watcher API"));

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
