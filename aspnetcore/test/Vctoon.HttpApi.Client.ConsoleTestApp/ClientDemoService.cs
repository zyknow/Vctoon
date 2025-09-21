using System;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace Vctoon.HttpApi.Client.ConsoleTestApp;

public class ClientDemoService(
    IProfileAppService profileAppService,
    IIdentityUserAppService identityUserAppService)
    : ITransientDependency
{
    public async Task RunAsync()
    {
        var profileDto = await profileAppService.GetAsync();
        Console.WriteLine($"UserName : {profileDto.UserName}");
        Console.WriteLine($"Email    : {profileDto.Email}");
        Console.WriteLine($"Name     : {profileDto.Name}");
        Console.WriteLine($"Surname  : {profileDto.Surname}");
        Console.WriteLine();

        var resultDto = await identityUserAppService.GetListAsync(new GetIdentityUsersInput());
        Console.WriteLine($"Total users: {resultDto.TotalCount}");
        foreach (var identityUserDto in resultDto.Items)
        {
            Console.WriteLine($"- [{identityUserDto.Id}] {identityUserDto.Name}");
        }
    }
}