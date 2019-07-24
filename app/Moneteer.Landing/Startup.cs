using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moneteer.Landing.V2.Helpers;
using Moneteer.Identity.Domain;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Moneteer.Landing.Repositories;
using Moneteer.Landing.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;

namespace Moneteer.Landing.V2
{
    public class Startup
    {
        private IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var appConnectionString = Configuration.GetConnectionString("App");
            services.AddSingleton(new DatabaseConnectionInfo { ConnectionString = Configuration.GetConnectionString("App") });
            services.AddSingleton<IConnectionProvider, ConnectionProvider>();
            services.AddTransient<IBudgetRepository, BudgetRepository>();

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Identity")));
            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAntiforgery();
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration.GetValue("CorsAllowedOrigins", String.Empty)).AllowAnyHeader().AllowAnyMethod();
                });
            });
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                
            })
            .AddCookie()
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = Configuration.GetValue<string>("OpenIdConnectAuthority");

                options.ClientId = "moneteer-mvc";

                options.RemoteAuthenticationTimeout = TimeSpan.FromHours(2);
                options.ResponseType = "id_token";
                options.RequireHttpsMetadata = !_env.IsDevelopment();
                options.Scope.Clear();
                options.Scope.Add("openid profile");

                options.CallbackPath = new PathString("/signin-callback-oidc");
                options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                options.SignedOutRedirectUri = new PathString("/");
                options.ClaimsIssuer = OpenIdConnectDefaults.AuthenticationScheme;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name
                };
            });

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton(new SmtpConnectionInfo
            {
                Host = Configuration["SmtpHost"],
                Username = Configuration["SmtpUsername"],
                Port = Configuration["SmtpPort"] == null ? 0 : int.Parse(Configuration["SmtpPort"]),
                Password = Configuration["SmtpPassword"],
                FromAddress = Configuration["SmtpFromAddress"]
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });

            services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders(); // See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-2.2

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();
        }
    }
}
