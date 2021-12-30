using Core.Data.Entities;
using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.Fishing
{
    public class FishingViewModel
    {
        public List<StakingPeriod> Periods { get; set; }
        public List<MyLakeViewModel> Lake { get; set; }
    }
    public class ItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ItemType Type { get; set; }
        public string TypeName { get; set; }
        public ItemGroup GroupItem { get; set; }
        public string GroupItemName { get; set; }
        public string ClassName { get; set; }
        public Status Status { get; set; }
        public string StatusName { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class MyItemViewModel
    {
        public int Id { get; set; }
        public int StakingTimeLineId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastReceived { get; set; }
        public Guid AppUserId { get; set; }
        public int TimeLine { get; set; }
        public decimal Interest { get; set; }
        public int ReceivedTime { get; set; }
        public decimal Price { get; set; }
        public StakingType Status { get; set; }
        public string StatusName { get; set; }
        public Guid ItemStakingId { get; set; }
        public AppUserView AppUser { set; get; }
        public ItemViewModel ItemStaking { set; get; }
        public  StakingTimeLine StakingTimeLine { set; get; }
        public  string StakingTimeLineName { set; get; }
        public Status StatusItemGame { get; set; }
        public string StatusItemGameName { get; set; }
        public string DateExpired { get; set; }
    }
    public class AppUserView
    {
        public decimal BNBBalance { get; set; }
        public decimal MARBalance { get; set; }
        public decimal MVRBalance { get; set; }
    }

    public class HistoryRewardViewModel
    {
        public decimal InterestRate { get; set; }
        public decimal Amount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public int StakingId { get; set; }
        public Guid AppUserId { get; set; }
        public ItemViewModel ItemInfo { get; set; }
    }

    public class MyLakeViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public ItemViewModel ItemInfo { get; set; }
    }
}
