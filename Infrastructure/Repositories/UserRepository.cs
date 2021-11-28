using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Entities;

namespace Infrastructure.Repositories
{
    public class UserRepository : EntityRepositoryBase<User>
    {
        public UserRepository(BotDbContext dbContext) : base(dbContext)
        {
        }
    }
}
