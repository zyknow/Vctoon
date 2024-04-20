using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.Permissions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;


public class LibraryAppService : CrudAppService<Library, LibraryDto, Guid, LibraryGetListInput, LibraryCreateUpdateDto, LibraryCreateUpdateDto>,
    ILibraryAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Library.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Library.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Library.Delete;

    private readonly ILibraryRepository _repository;

    public LibraryAppService(ILibraryRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override async Task<IQueryable<Library>> CreateFilteredQueryAsync(LibraryGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}
