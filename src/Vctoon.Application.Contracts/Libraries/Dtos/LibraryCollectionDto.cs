using System;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCollectionDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string CoverPath { get; set; }
}