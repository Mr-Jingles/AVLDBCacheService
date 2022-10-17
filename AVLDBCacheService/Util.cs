namespace AVLDBCacheService
{
    public class Util
    {
        //Returns number of unix ticks in future target is from reference.
        public static double CompareTime(DateTime target, DateTime reference)
        {
            return target.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds - reference.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        // Parses our date time format {0}d?{1}h?{2}m?{3}s? into DateTime
        public static DateTime ParseCacheLife(string rawstring)
        {
            var cacheLife = DateTime.Now;
            try
            {
                var days = rawstring.Split("d");
                if (days.Length > 1)
                {
                    cacheLife = cacheLife.AddDays(Double.Parse(days[0]));
                    rawstring = days[1];
                }
                var hours = rawstring.Split("h");
                if (hours.Length > 1)
                {
                    cacheLife = cacheLife.AddHours(Double.Parse(hours[0]));
                    rawstring = hours[1];
                }
                var minutes = rawstring.Split("m");
                if (minutes.Length > 1)
                {
                    cacheLife = cacheLife.AddMinutes(Double.Parse(minutes[0]));
                    rawstring = minutes[1];
                }
                var seconds = rawstring.Split("s");
                if (seconds.Length > 1)
                {
                    cacheLife = cacheLife.AddSeconds(Double.Parse(seconds[0]));
                }

                return cacheLife;
            } 
            catch (Exception e)
            {
                return DateTime.Now;
            }
        }
    }
}
