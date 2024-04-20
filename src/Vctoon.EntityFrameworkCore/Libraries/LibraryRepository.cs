using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class LibraryRepository : EfCoreRepository<VctoonDbContext, Library, Guid>, ILibraryRepository
{
    public LibraryRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Library>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}