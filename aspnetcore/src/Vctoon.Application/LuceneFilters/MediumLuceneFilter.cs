using System.Linq.Expressions;
using Lucene.Net.Search;
using Zyknow.Abp.Lucene.Filtering;

namespace Vctoon.LuceneFilters;

public class MediumLuceneFilter(LibraryPermissionStore libraryPermissionStore)
    : VctoonService, ILuceneFilterProvider
{
    
    static readonly string[] MediumEntityNames = new[]
    {
        nameof(Mediums.Comic),
        nameof(Mediums.Video)
    };
    
    public Task<Query?> BuildAsync(SearchFilterContext ctx)
    {
        if(!MediumEntityNames.Contains(ctx.EntityName, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult<Query?>(null);
        }
        
        var currentUserAccessLibraryIds = libraryPermissionStore.Permissions
            .Where(x => x.UserId == CurrentUser.Id)
            .Select(x => x.LibraryId).ToList();
        Expression<Func<MediumBaseProjection, bool>> exp = x => currentUserAccessLibraryIds.Contains(x.LibraryId);
        return Task.FromResult(LinqLucene.Where(ctx.Descriptor, exp));
    }

    // 只用于 LINQ 字段名解析的轻量投影类型
    private class MediumBaseProjection
    {
        public Guid LibraryId { get; set; }
    }
}