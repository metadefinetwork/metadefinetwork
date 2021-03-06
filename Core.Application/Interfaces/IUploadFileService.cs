using Core.Application.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IUploadFileService
    {
        Task<(int sumUpdated, List<MemberBalanceModel> notFoundMember)> ProcessUpdateInvestBalanceForMembers(ImportExcelModel model);
    }
}
