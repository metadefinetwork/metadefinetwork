using Core.Data.Entities;
using Core.Infrastructure.Interfaces;
using System;

namespace Core.Data.IRepositories
{
    public interface IItemGameRepository : IRepository<ItemGame, Guid>
    {
    }
}
