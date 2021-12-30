using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Application.ViewModels.Report
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
        }

        public int TotalMember { get; set; }
        public int TodayMember { get; set; }
        public int TotalMemberVerifyEmail { get; set; }
        public int TotalMemberInVerifyEmail { get; set; }

        public decimal TotalBNBDeposit { get; set; }
        public decimal TotalBNBWithdraw { get; set; }
        public decimal TodayBNBDeposit { get; set; }
        public decimal TodayBNBWithdraw { get; set; }

        public decimal TotalMARDeposit { get; set; }
        public decimal TotalMARWithdraw { get; set; }
        public decimal TodayMARDeposit { get; set; }
        public decimal TodayMARWithdraw { get; set; }

        public decimal TotalMVRDeposit { get; set; }
        public decimal TotalMVRWithdraw { get; set; }
        public decimal TodayMVRDeposit { get; set; }
        public decimal TodayMVRWithdraw { get; set; }
    }
}
