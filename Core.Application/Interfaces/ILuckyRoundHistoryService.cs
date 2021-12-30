using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ILuckyRoundHistoryService
    {
        PagedResult<LuckyRoundHistoryViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        int CountByType(LuckyRoundHistoryType type, int luckyRoundId);

        void Add(LuckyRoundHistoryViewModel model);

        void Save();
    }
}
