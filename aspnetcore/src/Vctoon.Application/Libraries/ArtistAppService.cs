using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;
using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;

namespace Vctoon.Libraries;

public class ArtistAppService(IArtistRepository repository)
    : VctoonCrudAppService<Artist, ArtistDto, Guid, ArtistGetListInput, ArtistCreateUpdateDto,
            ArtistCreateUpdateDto>(repository),
        IArtistAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Artist.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Artist.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Artist.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Artist.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Artist.Delete;

    protected override bool EnabledDataChangedHubNotify => true;

    protected override async Task<IQueryable<Artist>> CreateFilteredQueryAsync(ArtistGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            ;
    }
    
    public async Task DeleteManyAsync(List<Guid> ids)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteManyAsync(ids);
        
        UnitOfWorkManager.Current!.OnCompleted(async () =>
        {
            await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnDeleted,
                typeof(Artist).Name.ToLowerInvariant() , ids);
        });
    }
    

    [Route("/api/app/artist/all")]
    public async Task<List<ArtistDto>> GetAllTagAsync(bool withResourceCount = false)
    {
        await CheckGetListPolicyAsync();

        var tags = await AsyncExecuter.ToListAsync((await Repository.GetQueryableAsync()).Select(x =>
            new ArtistDto
            {
                Name = x.Name,
                Id = x.Id
            }));
        return tags;
    }
}