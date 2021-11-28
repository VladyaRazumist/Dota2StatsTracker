using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2StatsTracker.Exceptions
{
    public class PudgeBotException : Exception
    {
        public string FileForTipUrl { get; set; }

        public PudgeBotException()
        {
        }

        public PudgeBotException(string message)
            : base(message)
        {
        }

        public PudgeBotException(string message, string fileForTipUrl)
            : base(message)
        {
            FileForTipUrl = fileForTipUrl;
        }
    }
}
