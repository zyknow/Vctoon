using Microsoft.AspNetCore.Cors;
using Tailwind;
using Vctoon.Blazor.Client;
using Vctoon.Blazor.Components;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace Vctoon.Blazor;

[DependsOn(typeof(AbpAspNetCoreModule), typeof(VctoonBlazorClientModule), typeof(AbpAutofacModule),
    typeof(AbpHttpClientIdentityModelModule))]
public class VctoonBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        IConfiguration configuration = services.GetConfiguration();
        
        ConfigureCors(context, configuration);
        
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
    }
    
    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
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
            app.RunTailwind("watch-client", "./");
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseAbpRequestLocalization();
        
        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        
        app.UseRouting();
        // app.UseCors();
        app.UseAuthentication();
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        
        app.UseAntiforgery();
        
        var webApp = app as WebApplication;
        
        webApp.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            //.AddAdditionalAssemblies(typeof(Blazor.Client._Imports).Assembly);
            .AddAdditionalAssemblies(typeof(VctoonBlazorClientModule).Assembly);
    }
}