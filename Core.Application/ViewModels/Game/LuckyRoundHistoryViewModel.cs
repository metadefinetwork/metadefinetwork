using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;

namespace Core.Application.ViewModels.Game
{
    public class LuckyRoundHistoryViewModel
    {
        public int Id { get; set; }

        public string AddressFrom { get; set; }

        public string AddressTo { get; set; }

        public decimal Amount { get; set; }

        public LuckyRoundHistoryUnit Unit { get; set; }
        public string UnitName { get; set; }

        public LuckyRoundHistoryType Type { get; set; }
        public string TypeName { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public int LuckyRoundId { get; set; }
        public string Sponsor { get; set; }
        public Guid AppUserId { get; set; }
        public string AppUserName { get; set; }

        public AppUserViewModel AppUser { set; get; }

        public LuckyRoundViewModel LuckyRound { set; get; }
    }
}
