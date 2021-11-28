using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2StatsTracker.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UserRegisteredAttribute : Attribute
    {
    }
}
