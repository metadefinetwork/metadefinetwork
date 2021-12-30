using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface IChartRoundService
    {
        List<ChartRoundViewModel> GetAll();

        ChartRoundViewModel GetByType(ChartRoundType type);

        void Update(ChartRoundViewModel Model);

        void Save();
    }
}
