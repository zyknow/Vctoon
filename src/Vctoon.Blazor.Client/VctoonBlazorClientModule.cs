using Vctoon.RazorLibrary;
using Volo.Abp.Modularity;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(VctoonRazorLibraryModule))]
public class VctoonBlazorClientModule : AbpModule
{
}