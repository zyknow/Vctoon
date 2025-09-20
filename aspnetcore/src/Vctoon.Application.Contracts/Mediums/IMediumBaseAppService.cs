namespace Vctoon.Mediums;

public interface IMediumBaseAppService<TGetOutputDto, TGetListOutputDto, in TGetListInput, in TCreateInput,
    in TUpdateInput> : ICrudAppService<
    TGetOutputDto,
    TGetListOutputDto,
    Guid,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
{
}