using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class DotaAccount : EntityBase
    {
        public int SteamId { get; set; }
        public string PersonaName { get; set; }

        public List<DotaMatch> Matches { get; } = new List<DotaMatch>();
    }
}
