using Core.Application.Interfaces;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class LuckyRoundService : ILuckyRoundService
    {
        private readonly ILuckyRoundRepository _luckyRoundRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public LuckyRoundService(
            ILuckyRoundRepository luckyRoundRepository,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _luckyRoundRepository = luckyRoundRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public LuckyRoundViewModel GetProcess()
        {
            LuckyRound luckyRoundProcess = _luckyRoundRepository
                .FindAll(x => x.Status == LuckyRoundStatus.Process).FirstOrDefault();

            if (luckyRoundProcess == null)
            {
                luckyRoundProcess = new LuckyRound()
                {
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    Status = LuckyRoundStatus.Process
                };

                _luckyRoundRepository.Add(luckyRoundProcess);
                _unitOfWork.Commit();
            }

            return new LuckyRoundViewModel
            {
                Id = luckyRoundProcess.Id,
                Status = luckyRoundProcess.Status,
                DateCreated = luckyRoundProcess.DateCreated,
                DateUpdated = luckyRoundProcess.DateUpdated
            };
        }

        public void Update(LuckyRoundViewModel vm)
        {
            var luckyRound = _luckyRoundRepository.FindById(vm.Id);

            luckyRound.Status = vm.Status;
            luckyRound.DateUpdated = vm.DateUpdated;

            _luckyRoundRepository.Update(luckyRound);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
