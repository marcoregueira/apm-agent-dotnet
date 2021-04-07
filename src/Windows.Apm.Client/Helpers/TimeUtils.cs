using System;

namespace Elastic.Apm.Helpers
{
	public static class TimeUtils2
	{
		/// <summary>
		/// DateTime.UnixEpoch Field does not exist in .NET Standard 2.0
		/// https://docs.microsoft.com/en-us/dotnet/api/system.datetime.unixepoch
		/// </summary>
		public static readonly DateTime UnixEpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long TimestampNow() => ToTimestamp(DateTime.UtcNow).Value;

		/// <summary>
		/// UTC based and formatted as microseconds since Unix epoch.
		/// </summary>
		/// <param name="dateTimeToConvert">
		/// DateTime instance to convert to timestamp - its <see cref="DateTime.Kind" /> should be
		/// <see cref="DateTimeKind.Utc" />
		/// </param>
		/// <returns>UTC based and formatted as microseconds since Unix epoch</returns>
		public static long? ToTimestamp(DateTime? dateTime)
		{
			if (dateTime == null) return null;

			var dateTimeToConvert = dateTime.Value;
			if (dateTimeToConvert.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException($"{nameof(dateTimeToConvert)}'s Kind should be UTC but instead its Kind is {dateTimeToConvert.Kind}" +
					$". {nameof(dateTimeToConvert)}'s value: {dateTimeToConvert.FormatForLog()}", nameof(dateTimeToConvert));
			}

			return RoundTimeValue((dateTimeToConvert - UnixEpochDateTime).TotalMilliseconds * 1000);
		}

		public static DateTime ToDateTime(long timestamp) => UnixEpochDateTime + TimeSpan.FromTicks(timestamp * 10);

		public static string FormatTimestampForLog(long timestamp) => ToDateTime(timestamp).FormatForLog();

		/// <summary>
		/// Duration between timestamps in ms with 3 decimal points
		/// </summary>
		/// <returns>Duration between timestamps in ms with 3 decimal points</returns>
		public static double DurationBetweenTimestamps(long startTimestamp, long endTimestamp) => (endTimestamp - startTimestamp) / 1000.0;

		public static DateTime ToEndDateTime(long startTimestamp, double duration) =>
			ToDateTime(RoundTimeValue(startTimestamp + duration * 1000));

		public static TimeSpan TimeSpanFromFractionalMilliseconds(double fractionalMilliseconds) =>
			TimeSpan.FromTicks(RoundTimeValue(fractionalMilliseconds * TimeSpan.TicksPerMillisecond));

		public static long RoundTimeValue(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);
	}
}
