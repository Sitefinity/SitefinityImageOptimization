using System;

namespace Progress.Sitefinity.ImageOptimization.Utils
{
    internal interface IScheduleCalculator
    {
        DateTime? GetNextOccurrence(string scheduleSpec, DateTime time0);
    }
}