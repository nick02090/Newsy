using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAPI.Context;
using WebAPI.Repositories;
using WebAPI.Repositories.Interfaces;
using WebAPI.Settings;
using WebAPI.Settings.Interfaces;

namespace WebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<NewsyContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DB")));

            #region App Settings
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            // TODO: Configure authentication

            services.AddSingleton<IAppSettings>(appSettings);
            #endregion

            #region Connection settings
            var connectionSettings = new ConnectionSettings
            {
                ConnectionString = Configuration.GetConnectionString("NEWSY"),
                DbConnectionString = Configuration.GetConnectionString("DB")
            };

            services.AddSingleton<IConnectionSettings>(connectionSettings);
            #endregion

            #region Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            #endregion

            #region Services
            #endregion

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });

            services.AddResponseCompression();
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
