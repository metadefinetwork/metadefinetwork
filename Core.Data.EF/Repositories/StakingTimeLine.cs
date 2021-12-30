using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Data.EF.Repositories
{
    public class StakingPeriodRepository : EFRepository<StakingPeriod, int>, IStakingTimeLineRepository
    {
        public StakingPeriodRepository(AppDbContext context) : base(context)
        {
        }
    }
}
