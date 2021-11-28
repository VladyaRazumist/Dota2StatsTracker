using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dota2StatsTracker.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveExtraSpaces(this string str)
        {
            var regex = new Regex("[ ]{2,}");    
            
            return regex.Replace(str, " ");
        }
    }
}
