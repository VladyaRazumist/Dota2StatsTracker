using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FuzzySharp;

namespace Dota2StatsTracker.Helpers
{
    public static class FuzzyMatcher
    {
        private const int ExactMatchScoreBoundary = 100;
        private const int PossibleMatchScoreBoundary = 85;
        private const int PoorMatchScoreBoundary = 85;

        public static IReadOnlyCollection<string> FindMatchesByAbbreviation(IReadOnlyCollection<string> inputStrings, string strToMatchWith)
        {
            var exactMatches = new List<string>();

            foreach (var str in inputStrings)
            {
                var match = Fuzz.TokenInitialismRatio(strToMatchWith, str);

                if (match == ExactMatchScoreBoundary)
                    exactMatches.Add(str);
            }

            return exactMatches;
        }

        public static IReadOnlyCollection<string> ExtractTop(IReadOnlyCollection<string> inputStrings, string strToMatchWith, int limit = 3, bool poorMatching = false)
        {
            var matchBoundary = poorMatching ? PoorMatchScoreBoundary : PossibleMatchScoreBoundary;

           return Process.ExtractTop(strToMatchWith, inputStrings, limit: limit)
                .Where(m => m.Score > matchBoundary).Select(m => m.Value).ToArray();
        }

    }
}
