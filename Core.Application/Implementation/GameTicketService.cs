using Core.Application.Interfaces;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using System;
using System.Linq;

namespace Core.Application.Implementation
{
    public class GameTicketService : IGameTicketService
    {
        private readonly IGameTicketRepository _gameTicketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GameTicketService(
            IGameTicketRepository gameTicketRepository,
            IUnitOfWork unitOfWork)
        {
            _gameTicketRepository = gameTicketRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<GameTicketViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _gameTicketRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Value.ToString().Contains(keyword));

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new GameTicketViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Sponsor = $"{ x.AppUser.Sponsor}",
                    DateCreated = x.DateCreated,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                }).ToList();

            return new PagedResult<GameTicketViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(GameTicketViewModel model)
        {
            var transaction = new GameTicket()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,
                Amount = model.Amount,
                AppUserId = model.AppUserId,
                DateCreated = model.DateCreated,
                Type = model.Type
            };

            _gameTicketRepository.Add(transaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
