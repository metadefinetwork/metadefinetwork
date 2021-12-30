using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface IGameTicketService
    {
        PagedResult<GameTicketViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        void Add(GameTicketViewModel Model);

        void Save();
    }
}
