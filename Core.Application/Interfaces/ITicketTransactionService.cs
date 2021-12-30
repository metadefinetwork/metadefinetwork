using Core.Application.ViewModels.System;
using Core.Utilities.Dtos;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ITicketTransactionService
    {
        PagedResult<TicketTransactionViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        void Add(TicketTransactionViewModel Model);

        Task Rejected(int id);

        Task<GenericResult> Approved(int id);

        void Save();
    }
}
