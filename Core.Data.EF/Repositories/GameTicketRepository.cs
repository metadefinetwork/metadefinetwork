﻿using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Data.EF.Repositories
{
    public class GameTicketRepository : EFRepository<GameTicket, int>, IGameTicketRepository
    {
        public GameTicketRepository(AppDbContext context) : base(context)
        {
        }
    }
}
