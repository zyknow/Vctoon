using System;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface IFavoriteRepository : IRepository<Favorite, Guid>
{
}