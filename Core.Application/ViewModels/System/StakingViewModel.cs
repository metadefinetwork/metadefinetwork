using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.System
{
    public class StakingViewModel
    {
        public StakingViewModel()
        {
            StakingRewards = new List<StakingRewardViewModel>();
        }

        public int Id { get; set; }
        public bool IsGetedCommission { get; set; }
        public StakingTimeLine TimeLine { get; set; }
        public string TimeLineName { get; set; }

        public StakingPackage Package { get; set; }
        public string PackageName { get; set; }

        public int InterestRate { get; set; }

        public decimal StakingAmount { get; set; }
        public int StakingTimes { get; set; }

        public decimal ReceiveAmount { get; set; }
        public int ReceiveTimes { get; set; }

        public StakingType Type { get; set; }
        public string TypeName { get; set; }

        public DateTime ReceiveLatest { get; set; }

        public DateTime DateCreated { get; set; }
        public string Sponsor { get; set; }

        public string AppUserName { get; set; }
        public Guid AppUserId { get; set; }

        public AppUserViewModel AppUser { set; get; }
        public List<StakingRewardViewModel> StakingRewards { get; set; }
    }
}
