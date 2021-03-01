using System;

using Sitecore;
using Sitecore.Common;
using Sitecore.Configuration;

namespace Examples.ContentEditorCulture.Utils
{
    public sealed class TimeZoneUtil
    {
        public static string IsoDateToUserTimeIsoDate(string isoDate)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(isoDate))
            {
                result = DateUtil.ToIsoDate(ToUserTime(DateUtil.IsoDateToDateTime(isoDate)));
            }

            return result;
        }

        public static DateTime ToUserTime(DateTime timeToConvert)
        {
            DateTime result;
            TimeZoneInfo userTimezone = GetCurrentUserTimeZoneInfo();
            if (timeToConvert == DateTime.MinValue || timeToConvert == DateTime.MaxValue)
            {
                result = timeToConvert;
            }
            else
            {
                // ReSharper disable once SwitchStatementMissingSomeCases - On purpose
                switch (timeToConvert.Kind)
                {
                    case DateTimeKind.Utc:
                        result = TimeZoneInfo.ConvertTimeFromUtc(timeToConvert, userTimezone);
                        break;
                    case DateTimeKind.Local:
                        result = TimeZoneInfo.ConvertTimeFromUtc(timeToConvert.ToUniversalTime(), userTimezone);
                        break;
                    default:
                        result = timeToConvert;
                        break;
                }
            }

            return result.SpecifyKind(DateTimeKind.Unspecified);
        }

        public static TimeZoneInfo GetCurrentUserTimeZoneInfo()
        {
            string timezone = Context.User?.Profile?.GetCustomProperty(Constants.TimezoneUserProfileFieldKey);
            return !string.IsNullOrWhiteSpace(timezone) ? TimeZoneInfo.FindSystemTimeZoneById(timezone) : Settings.ServerTimeZone;
        }

        public static string IsoDateToUtcIsoDate(string isoDate)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(isoDate))
            {
                result = DateUtil.ToIsoDate(ToUniversalTime(DateUtil.IsoDateToDateTime(isoDate)));
            }

            return result;
        }

        public static DateTime ToUniversalTime(DateTime userTime)
        {
            DateTime result;
            TimeZoneInfo userTimezone = GetCurrentUserTimeZoneInfo();
            if (userTime == DateTime.MinValue || userTime == DateTime.MaxValue)
            {
                result = userTime.SpecifyKind(DateTimeKind.Utc);
            }
            else if (userTime.Kind == DateTimeKind.Utc)
            {
                result = userTime;
            }
            else
            {
                result = TimeZoneInfo.ConvertTimeToUtc(userTime, userTimezone);
            }

            return result;
        }
    }
}