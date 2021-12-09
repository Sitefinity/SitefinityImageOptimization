using NCrontab;
using System;

namespace Progress.Sitefinity.ImageOptimization.Utils
{
    internal class CrontabScheduleCalculator : IScheduleCalculator
    {
        public DateTime? GetNextOccurrence(string scheduleSpec, DateTime baseTime)
        {
            int? year;
            ParseYearField(ref scheduleSpec, out year);

            ValueOrError<CrontabSchedule> sch = CrontabSchedule.TryParse(scheduleSpec);
            if (sch.IsError)
                throw sch.Error;

            DateTime endTime = DateTime.MaxValue;

            // If an year is specified, restrict the next occurrences to that specific year.
            if (year != null)
            {
                if (year.Value != baseTime.Year)
                    baseTime = new DateTime(year.Value, 1, 1, 0, 0, 0, baseTime.Kind);
                endTime = new DateTime(year.Value, 12, 31, 23, 59, 0, baseTime.Kind);
            }

            DateTime nextTime = sch.Value.GetNextOccurrence(baseTime, endTime);

            if (nextTime == endTime)
                return null;

            return nextTime;
        }

        private static void ParseYearField(ref string spec, out int? year)
        {
            year = null;

            string[] fields = spec.Split();
            if (fields.Length > maxFieldCount)
                throw new FormatException("Too many fields in crontab expression: " + spec);

            if (fields.Length > yearFieldIndex)
            {
                string yearField = fields[yearFieldIndex];
                if (yearField != "*")
                {
                    int yearFieldValue;
                    if (!int.TryParse(yearField, out yearFieldValue))
                        throw new FormatException("Invalid year field in crontab expression: " + yearField);

                    year = yearFieldValue;
                }

                spec = string.Join(" ", fields, 0, 5);
            }
        }

        public const string ScheduleSpecType = "crontab";

        private const int maxFieldCount = 6;
        private const int yearFieldIndex = 5;
    }
}
