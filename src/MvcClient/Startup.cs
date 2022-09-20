using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace MvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
                {
                    o.Cookie.Name = "auth.";
                    o.Cookie.IsEssential = true;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.ClientId = "mvc";
                    options.Scope.Add("profile");
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Prompt = "login";
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.Authority = "https://localhost:5001";

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProvider = ctx =>
                        {
                            var tenant = ctx.HttpContext.GetMultiTenantContext<AppTenantInfo>()?.TenantInfo?.Identifier;
                            ctx.ProtocolMessage.AcrValues = $"tenant:{tenant}";
                            return Task.FromResult(0);
                        }
                    };
                });

            services.AddMultiTenant<AppTenantInfo>()
                .WithRemoteAuthenticationCallbackStrategy()
                .WithConfigurationStore()
                .WithRouteStrategy()
                .WithPerTenantAuthentication()
                .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenantContext) =>
                {
                    options.Cookie.Name = $"auth.{tenantContext.Identifier}";
                    options.Cookie.Path = "/" + tenantContext.Identifier;
                    options.LoginPath = "/" + tenantContext.Identifier + "/Home/Login";
                });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMultiTenant();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{__tenant__=}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
