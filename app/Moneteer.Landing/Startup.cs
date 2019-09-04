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
using Microsoft.AspNetCore.Identity.UI.Services;
using Moneteer.Landing.V2.Helpers;
using Moneteer.Identity.Domain;
using Moneteer.Identity.Domain.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Moneteer.Landing.Repositories;
using Moneteer.Landing.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
using Serilog;
using Stripe;
using Moneteer.Landing.Managers;

namespace Moneteer.Landing.V2
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; } 
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Stripe.StripeConfiguration.ApiKey = Configuration["Stripe:ApiKey"];

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Domain = Environment.IsDevelopment() ? "" : null;
            });

            var moneteerConnectionString = Configuration.GetConnectionString("Moneteer");
            services.AddSingleton(new DatabaseConnectionInfo { ConnectionString = moneteerConnectionString });
            services.AddSingleton<IConnectionProvider, ConnectionProvider>();
            services.AddTransient<IBudgetRepository, BudgetRepository>();
            services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();

            services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(moneteerConnectionString));
            services.AddDbContext<DataProtectionKeysContext>(options => options.UseNpgsql(moneteerConnectionString));
            services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<IdentityDbContext>();


            // Data Protection - Provides storage and encryption for anti-forgery tokens
            if (Environment.IsDevelopment())
            {
                services.AddDataProtection()
                        .PersistKeysToDbContext<DataProtectionKeysContext>();
            }
            else 
            {
                services.AddDataProtection()
                        .PersistKeysToDbContext<DataProtectionKeysContext>()
                        .ProtectKeysWithCertificate(GetSigningCertificate());
            }
    
            services.AddAntiforgery();
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration["AllowedCorsOrigins"]).AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

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
                options.Authority = Configuration["OpenIdConnectAuthority"];

                options.ClientId = "moneteer-mvc";
                options.ClientSecret = Configuration["ClientSecret"];

                options.RemoteAuthenticationTimeout = TimeSpan.FromHours(2);
                options.ResponseType = "code id_token";
                options.RequireHttpsMetadata = !Environment.IsDevelopment();
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

            services.AddMvc(options => 
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSerilogRequestLogging();
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            } 

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();
        }

        private X509Certificate2 GetSigningCertificate()
        {
            var cert = Configuration["TokenSigningCert"];
            var secret = Configuration["TokenSigningCertSecret"];
                
            byte[] decodedPfxBytes = Convert.FromBase64String(cert);
            return new X509Certificate2(decodedPfxBytes, secret);
        }
    }
}
