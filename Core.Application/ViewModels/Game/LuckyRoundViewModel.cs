using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.Game
{
    public class LuckyRoundViewModel
    {
        public LuckyRoundViewModel()
        {
            LuckyRoundHistories = new List<LuckyRoundHistoryViewModel>();
        }

        public int Id { get; set; }

        public LuckyRoundStatus Status { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public List<LuckyRoundHistoryViewModel> LuckyRoundHistories { get; set; }
    }
}
