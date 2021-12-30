using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.Fishing
{
    public class ItemListingRequest
    {
        public string KeyWord { get; set; }
        public int Type { get; set; }
        public int Group { get; set; }
        public decimal Price { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    public class MyItemListingRequest
    {
        public int GroupId { get; set; }
        public string AppUserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    public class HistoryRewardListingRequest
    {
        public string AppUserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
