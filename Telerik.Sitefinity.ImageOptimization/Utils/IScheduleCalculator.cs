using System;

namespace Telerik.Sitefinity.ImageOptimization.Utils
{
    internal interface IScheduleCalculator
    {
        DateTime? GetNextOccurrence(string scheduleSpec, DateTime time0);
    }
}