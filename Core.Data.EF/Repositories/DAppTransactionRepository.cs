using Core.Data.Entities;
using Core.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Core.Data.EF.Repositories
{
    public class DAppTransactionRepository : EFRepository<DAppTransaction, Guid>, IDAppTransactionRepository
    {
        public DAppTransactionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
