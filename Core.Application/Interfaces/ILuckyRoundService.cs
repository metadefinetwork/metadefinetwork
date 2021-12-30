using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Utilities.Dtos;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ILuckyRoundService
    {
        void Update(LuckyRoundViewModel vm);

        LuckyRoundViewModel GetProcess();

        void Save();
    }
}
