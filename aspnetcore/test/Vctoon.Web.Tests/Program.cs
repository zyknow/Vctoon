using Microsoft.AspNetCore.Builder;
using Vctoon;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("Vctoon.Web.csproj");
await builder.RunAbpModuleAsync<VctoonWebTestModule>(applicationName: "Vctoon.Web");

namespace Vctoon
{
    public partial class Program
    {
    }
}