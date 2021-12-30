using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.Staking
{
    public class BuyItemGameViewModel
    {
        public string ItemId { get; set; }
        public int PeriodId { get; set; }
        public int WalletType { get; set; }
    }
    public class BuyStakingViewModel
    {
        public int Type { get; set; }
        public StakingTimeLine TimeLine { get; set; }
        public StakingPackage Package { get; set; }
        public decimal AmountPayment { get; set; }
    }
}
