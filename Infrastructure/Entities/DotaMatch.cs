using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class DotaMatch : EntityBase
    {
        public long OpenDotaMatchId { get; set; }

        public int DotaAccountId { get; set; }
        public DotaAccount DotaAccount { get; set; }
    }
}
