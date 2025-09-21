using Vctoon.Mediums.Dtos.Base;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums.Base;

public abstract class MediumBaseAppService<TEntity, TGetOutputDto, TGetListOutputDto, TGetListInput, TCreateInput,
    TUpdateInput>(
    IRepository<TEntity, Guid> repository)
    : VctoonCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, Guid, TGetListInput, TCreateInput,
        TUpdateInput>(repository), IMediumBaseAppService<TGetOutputDto, TGetListOutputDto, TGetListInput, TCreateInput,
        TUpdateInput>
    where TEntity : MediumBase
    where TGetOutputDto : MediumDtoBase
    where TGetListOutputDto : MediumGetListOutputDtoBase
    where TGetListInput : MediumGetListInputBase
    where TCreateInput : MediumCreateUpdateDtoBase
    where TUpdateInput : MediumCreateUpdateDtoBase
{
    protected override async Task<IQueryable<TEntity>> CreateFilteredQueryAsync(TGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(!input.Description.IsNullOrWhiteSpace(), x => x.Description.Contains(input.Description))
            .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            .WhereIf(!input.Artists.IsNullOrEmpty(), x => x.Artists.Any(a => input.Artists!.Contains(a.Id)))
            .WhereIf(!input.Tags.IsNullOrEmpty(), x => x.Tags.Any(t => input.Tags!.Contains(t.Id)))
            ;
    }
}