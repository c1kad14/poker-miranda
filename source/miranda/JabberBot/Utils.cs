namespace miranda.JabberBot
{
    public class Utils
    {
        public static bool TryParse<T>(object value)

        {
            T x;
            try
            {
                x = (T) value;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}