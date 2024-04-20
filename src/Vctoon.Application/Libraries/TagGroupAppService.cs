using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.Permissions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;


public class TagGroupAppService : CrudAppService<TagGroup, TagGroupDto, Guid, TagGroupGetListInput, TagGroupCreateUpdateDto, TagGroupCreateUpdateDto>,
    ITagGroupAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.TagGroup.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.TagGroup.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.TagGroup.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.TagGroup.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.TagGroup.Delete;

    private readonly ITagGroupRepository _repository;

    public TagGroupAppService(ITagGroupRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override async Task<IQueryable<TagGroup>> CreateFilteredQueryAsync(TagGroupGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}
