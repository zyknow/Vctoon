using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.Permissions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public class TagAppService : CrudAppService<Tag, TagDto, Guid, TagGetListInput, CreateUpdateTagDto, CreateUpdateTagDto>,
    ITagAppService
{
    private readonly ITagRepository _repository;

    public TagAppService(ITagRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override string GetPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Tag.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Tag.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Tag.Delete;

    protected override async Task<IQueryable<Tag>> CreateFilteredQueryAsync(TagGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}