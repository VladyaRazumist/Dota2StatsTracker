using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Entities;

namespace Infrastructure.Repositories
{
    public class DotaMatchRepository : EntityRepositoryBase<DotaMatch>
    {
        public DotaMatchRepository(BotDbContext dbContext) : base(dbContext)
        {
        }
    }
}
