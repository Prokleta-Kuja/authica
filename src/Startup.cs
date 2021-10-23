using System;
using System.Diagnostics;
using authica.Entities;
using authica.Jobs;
using authica.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace authica
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.ScheduleJob<GeolocationDbDownloadJob>(trigger => trigger
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(7)))
                );
            });
            services.AddQuartzServer(q => q.WaitForJobsToComplete = true);

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuth.Configure);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddDbContextFactory<AppDbContext>(builder =>
            {
                builder.UseSqlite(C.Paths.AppDbConnectionString);
                if (Debugger.IsAttached)
                {
                    builder.EnableSensitiveDataLogging();
                    builder.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
                }
            });
            services.AddDbContext<AppDbContext>(builder =>
            {
                builder.UseSqlite(C.Paths.AppDbConnectionString);
                if (Debugger.IsAttached)
                {
                    builder.EnableSensitiveDataLogging();
                    builder.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
                }
            });

            services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
            services.AddSingleton<IPasswordHasher, PasswordHashingService>();
            services.AddSingleton<MailService>();
            services.AddScoped<ToastService>();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddScoped<IpSecurity>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
            }

            app.UseIpSecurity();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapRazorPages().RequireAuthorization();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}