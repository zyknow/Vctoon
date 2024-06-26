﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using Vctoon.ImageProviders;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Vctoon;

[DependsOn(
    typeof(VctoonDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(VctoonApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
)]
public class VctoonApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<VctoonApplicationModule>(); });
        context.Services.TryAddTransient<IImageProvider, ImageRealFileProvider>();
    }
}