using Hangfire;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Vctoon.BlobContainers;
using Vctoon.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Vctoon;

[DependsOn(
    typeof(VctoonHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(VctoonApplicationModule),
    typeof(VctoonEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpBlobStoringFileSystemModule),
    // typeof(AbpBackgroundJobsHangfireModule),
    typeof(AbpAspNetCoreSignalRModule)
)]
public class VctoonHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        IWebHostEnvironment hostingEnvironment = context.Services.GetHostingEnvironment();
        IConfiguration configuration = context.Services.GetConfiguration();
        
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Vctoon");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
        
        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });
            
            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx",
                    "b5fc7d59-ce7c-4a15-b33b-005f6f373d4f");
            });
        }
    }
    
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        IConfiguration configuration = context.Services.GetConfiguration();
        IWebHostEnvironment hostingEnvironment = context.Services.GetHostingEnvironment();
        IServiceCollection services = context.Services;
        
        ConfigureAuthentication(context);
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureBlobStoring();
        // ConfigureHangfire(context, configuration);
    }
    
    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddHangfire(config => { config.UseSQLiteStorage(configuration.GetConnectionString("Default")); });
        
        context.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = 20;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        });
        
        context.Services.AddHangfireServer(options =>
        {
            options.Queues = new[] {"scan-library"};
            options.WorkerCount = 1;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        });
    }
    
    private void ConfigureBlobStoring()
    {
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<CoverContainer>(container =>
            {
                container.UseFileSystem(fileSystem => { fileSystem.BasePath = @$".{Path.DirectorySeparatorChar}"; });
            });
        });
    }
    
    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults
            .AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options => { options.IsDynamicClaimsEnabled = true; });
    }
    
    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); }
            );
        });
    }
    
    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ??
                                                 Array.Empty<string>());
            
            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }
    
    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        IWebHostEnvironment hostingEnvironment = context.Services.GetHostingEnvironment();
        
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Vctoon.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Vctoon.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Vctoon.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Vctoon.Application"));
            });
        }
    }
    
    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(VctoonApplicationModule).Assembly);
        });
    }
    
    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        // context.Services.AddAbpSwaggerGen(
        //     options =>
        //     {
        //         options.SwaggerDoc("v1", new OpenApiInfo {Title = "Vctoon API", Version = "v1"});
        //         options.DocInclusionPredicate((docName, description) => true);
        //         options.CustomSchemaIds(type => type.FullName);
        //     }
        // );
        //
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                {"Vctoon", "Vctoon API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo {Title = "Vctoon API", Version = "v1"});
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
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
        IApplicationBuilder app = context.GetApplicationBuilder();
        IWebHostEnvironment env = context.GetEnvironment();
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseAbpRequestLocalization();
        
        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }
        
        
        // app.UseHangfireDashboard();
        
        app.UseHttpsRedirection();
        app.UseCorrelationId();
        
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        
        app.UseAntiforgery();
        
        app.UseSwagger();
        
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vctoon API");
            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("Vctoon");
        });
        
        app.UseAuditing();
        
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}