using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Tailwind;
using Vctoon.Blazor.Client;
using Vctoon.Blazor.Components;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Authentication.OpenIdConnect;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Client;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Http.Client.IdentityModel.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;

namespace Vctoon.Blazor;

[DependsOn(
    typeof(AbpAspNetCoreModule),
    typeof(VctoonBlazorClientModule),
    typeof(AbpAutofacModule),
    typeof(AbpHttpClientIdentityModelModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAspNetCoreAuthenticationOpenIdConnectModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAspNetCoreMvcClientModule)
)]
public class VctoonBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        IConfiguration configuration = services.GetConfiguration();
        
        ConfigureAuthentication(context, configuration);
        
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
    }
    
    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(365);
            })
            .AddAbpOpenIdConnect("oidc",options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                
                options.ClientId = configuration["AuthServer:ClientId"];
                options.ClientSecret = configuration["AuthServer:ClientSecret"];
                
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                
                options.Scope.Add("roles");
                options.Scope.Add("email");
                options.Scope.Add("phone");
                options.Scope.Add("Vctoon");
                
            });
        /*
         * This configuration is used when the AuthServer is running on the internal network such as docker or k8s.
         * Configuring the redirecting URLs for internal network and the web
         * The login and the logout URLs are configured to redirect to the AuthServer real DNS for browser.
         * The token acquired and validated from the the internal network AuthServer URL.
         */
        if (configuration.GetValue<bool>("AuthServer:IsContainerized"))
        {
            context.Services.Configure<OpenIdConnectOptions>("oidc", options =>
            {
                options.TokenValidationParameters.ValidIssuers = new[]
                {
                    configuration["AuthServer:MetaAddress"]!.EnsureEndsWith('/'),
                    configuration["AuthServer:Authority"]!.EnsureEndsWith('/')
                };
                
                options.MetadataAddress = configuration["AuthServer:MetaAddress"]!.EnsureEndsWith('/') +
                                          ".well-known/openid-configuration";
                
                var previousOnRedirectToIdentityProvider = options.Events.OnRedirectToIdentityProvider;
                options.Events.OnRedirectToIdentityProvider = async ctx =>
                {
                    // Intercept the redirection so the browser navigates to the right URL in your host
                    ctx.ProtocolMessage.IssuerAddress =
                        configuration["AuthServer:Authority"]!.EnsureEndsWith('/') + "connect/authorize";
                    
                    if (previousOnRedirectToIdentityProvider != null)
                    {
                        await previousOnRedirectToIdentityProvider(ctx);
                    }
                };
                var previousOnRedirectToIdentityProviderForSignOut = options.Events.OnRedirectToIdentityProviderForSignOut;
                options.Events.OnRedirectToIdentityProviderForSignOut = async ctx =>
                {
                    // Intercept the redirection for signout so the browser navigates to the right URL in your host
                    ctx.ProtocolMessage.IssuerAddress =
                        configuration["AuthServer:Authority"]!.EnsureEndsWith('/') + "connect/logout";
                    
                    if (previousOnRedirectToIdentityProviderForSignOut != null)
                    {
                        await previousOnRedirectToIdentityProviderForSignOut(ctx);
                    }
                };
            });
        }
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
            options.RemoteRefreshUrl = configuration["AuthServerUrl"] + options.RemoteRefreshUrl;
        });
    }
    
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.RunTailwind("watch");
        }
        
        app.UseAbpRequestLocalization();
        
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        
        // app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        // app.UseDynamicClaims();
        app.UseAuthorization();
        
        app.UseAntiforgery();
        
        var webApp = app as WebApplication;
        
        webApp.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(VctoonBlazorClientModule).Assembly);
        
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}