﻿using Core.Data.Entities;
using Core.Infrastructure.Interfaces;

namespace Core.Data.IRepositories
{
    public interface IGameTicketRepository : IRepository<GameTicket, int>
    {
    }
}