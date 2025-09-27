using System.Collections.Generic;

namespace Vctoon.Hubs;

public class DataChangedHubMessage<TEntityDto>(string type, List<TEntityDto> items)
{
    public string Type { get; set; } = type;

    public List<TEntityDto> Items { get; set; } = items;
}