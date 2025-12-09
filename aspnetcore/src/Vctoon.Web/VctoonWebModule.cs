using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Vctoon.BlobContainers;
using Vctoon.EntityFrameworkCore;
using Vctoon.Localization.Vctoon;
using Vctoon.Mediums;
using Vctoon.Web.HealthChecks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Options;

namespace Vctoon.Web;

[DependsOn(
    typeof(VctoonHttpApiModule),
    typeof(VctoonApplicationModule),
    typeof(VctoonEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityAspNetCoreModule),
    // typeof(AbpIdentityWebModule),
    // typeof(AbpOpenIddictAspNetCoreModule),
    typeof(AbpAccountWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(AbpBlobStoringFileSystemModule)
)]
public class VctoonWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(VctoonResource),
                typeof(VctoonDomainModule).Assembly,
                typeof(VctoonDomainSharedModule).Assembly,
                typeof(VctoonApplicationModule).Assembly,
                typeof(VctoonApplicationContractsModule).Assembly,
                typeof(VctoonWebModule).Assembly
            );
        });

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
                    configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            IdentityModelEventSource.ShowPII = true;
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });
        }

        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureHealthChecks(context);
        ConfigureAuthentication(context);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        ConfigureCors(context, configuration);

        Configure<PermissionManagementOptions>(options => { options.IsDynamicPermissionStoreEnabled = true; });

        Configure<AbpMvcLibsOptions>(options => { options.CheckLibs = false; });

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<CoverContainer>(container =>
            {
                container.UseFileSystem(fileSystem => { fileSystem.BasePath = @$"Covers"; });
            });
        });

        Configure<LuceneOptions>(opt =>
        {
            // opt.IndexRootPath = Path.Combine(AppContext.BaseDirectory, "lucene-index");
            opt.IndexRootPath = "lucene-index";
            opt.PerTenantIndex = false;
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Medium>(e =>
                {
                    e.Field(x => x.Title, f => f.Store());
                    e.Field(x => x.Cover, f => f.StoreOnly());
                    e.Field(x => x.LibraryId, f => f.Keyword());
                    e.Field(x => x.LibraryPathId, f => f.Keyword());
                    e.Field(x => x.MediumType, f => f.Keyword());
                });
            });
        });
    }


    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddVctoonHealthChecks();
    }

    private void ConfigureBundles()
    {
        // Configure<AbpBundlingOptions>(options =>
        // {
        //     options.StyleBundles.Configure(
        //         BasicThemeBundles.Styles.Global,
        //         bundle =>
        //         {
        //             bundle.AddFiles("/global-styles.css");
        //         }
        //     );
        //
        //     options.ScriptBundles.Configure(
        //         BasicThemeBundles.Scripts.Global,
        //         bundle =>
        //         {
        //             bundle.AddFiles("/global-scripts.js");
        //         }
        //     );
        // });
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

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options => { options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"]; });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults
            .AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<VctoonWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        string.Format("..{0}Vctoon.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonDomainModule>(Path.Combine(
                    hostingEnvironment.ContentRootPath,
                    string.Format("..{0}Vctoon.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        string.Format("..{0}Vctoon.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        string.Format("..{0}Vctoon.Application", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonHttpApiModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        string.Format("..{0}..{0}src{0}Vctoon.HttpApi", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<VctoonWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(VctoonApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Vctoon API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            // app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();
        app.UseCors();
        app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vctoon API"); });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}