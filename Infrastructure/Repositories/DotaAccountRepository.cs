using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Entities;

namespace Infrastructure.Repositories
{
    public class DotaAccountRepository : EntityRepositoryBase<DotaAccount>
    {
        public DotaAccountRepository(BotDbContext dbContext) : base(dbContext)
        {
        }
    }
}
