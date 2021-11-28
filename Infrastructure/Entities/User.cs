using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class User : EntityBase
    {
        public ulong DiscordUserId { get; set; }
        public string Name { get; set; }

        public int? AccountId { get; set; }
        public DotaAccount Account { get; set; }
    }
}
