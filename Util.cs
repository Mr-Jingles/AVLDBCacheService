namespace AVLDBCacheService
{
    public class Util
    {
        //Returns number of unix ticks in future target is from reference.
        public static double CompareTime(DateTime target, DateTime reference)
        {
            return target.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds - reference.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;
        }
    }
}
