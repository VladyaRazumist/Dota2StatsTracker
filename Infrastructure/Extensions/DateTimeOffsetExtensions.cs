using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static bool IsEmpty(this DateTimeOffset self)
        {
            return self == default;
        }
    }
}
