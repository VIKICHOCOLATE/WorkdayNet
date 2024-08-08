namespace WorkdayNet
{
	internal class WorkdayCalendar : IWorkdayCalendar
	{
		private HashSet<DateTime> holidays = new();
		private HashSet<(int month, int day)> recurringHolidays = new();
		private TimeSpan workdayStart;
		private TimeSpan workdayStop;

		private DateTime AddWorkDays(DateTime date, int days)
		{
			int direction = Math.Sign(days);
			days = Math.Abs(days);

			while (days > 0)
			{
				date = date.AddDays(direction);

				if (IsWorkday(date))
					days--;
			}

			return date;
		}

		private bool IsWorkday(DateTime date)
		{
			return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday
				&& !holidays.Contains(date.Date) && !recurringHolidays.Contains((date.Month, date.Day));
		}

		private DateTime AddFractionalWorkDay(DateTime date, decimal fractionalDay)
		{
			int totalMinutes = (int)Math.Round((workdayStop - workdayStart).TotalMinutes * (double)fractionalDay);
			TimeSpan increment = TimeSpan.FromMinutes(totalMinutes);

			while (increment != TimeSpan.Zero)
			{
				if (date.TimeOfDay + increment < workdayStop)
				{
					date = date.Add(increment);
					increment = TimeSpan.Zero;
				}
				else
				{
					TimeSpan remaining = workdayStop - date.TimeOfDay;
					date = AddWorkDays(date.Date.Add(workdayStart).AddDays(1), 1);
					increment -= remaining;
				}
			}

			return date;
		}

		public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
		{
			// Ensure start date is within work hours
			if (startDate.TimeOfDay < workdayStart)
				startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workdayStart.Hours, workdayStart.Minutes, 0);
			if (startDate.TimeOfDay > workdayStop)
				startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, workdayStop.Hours, workdayStop.Minutes, 0);

			int wholeDays = (int)incrementInWorkdays;
			decimal fractionalDay = incrementInWorkdays - wholeDays;

			DateTime resultDate = startDate;

			// Add whole days
			resultDate = AddWorkDays(resultDate, wholeDays);

			// Add fractional day
			resultDate = AddFractionalWorkDay(resultDate, fractionalDay);

			return resultDate;
		}

		public void SetHoliday(DateTime date)
		{
			holidays.Add(date.Date);
		}

		public void SetRecurringHoliday(int month, int day)
		{
			recurringHolidays.Add((month, day));
		}

		public void SetWorkdayStartAndStop(int startHours, int startMinutes, 
			int stopHours, int stopMinutes)
		{
			workdayStart = new TimeSpan(startHours, startMinutes, 0);
			workdayStop = new TimeSpan(stopHours, stopMinutes, 0);
		}
	}
}
