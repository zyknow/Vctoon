using System;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryDto : EntityDto<Guid>
{
    public string Name { get; set; }
}