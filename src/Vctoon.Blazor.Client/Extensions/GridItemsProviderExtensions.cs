using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;

namespace Vctoon.Blazor.Client.Extensions;

public static class GridItemsProviderExtensions
{
    public static string GetSorting<T>(this GridItemsProviderRequest<T> request)
    {
        IReadOnlyCollection<SortedProperty> sortByProperties = request.GetSortByProperties();

        if (request.SortByColumn == null || sortByProperties.IsNullOrEmpty())
        {
            return "";
        }

        return $@"{request.GetSortByProperties().First().PropertyName} {(request.SortByAscending ? "desc" : "asc")}";
    }
}