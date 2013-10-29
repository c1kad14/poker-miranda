using System.Drawing;
using System.Globalization;

namespace miranda.ui
{
    public class Bank
    {
        public Bank()
        {
            BankStr = "";
            CallStr = "";
            RaiseStr = "";
            TotalStr = "";
        }
        public string BankStr { get; set; }
        public string CallStr { get; set; }
        public string RaiseStr { get; set; }
        public string TotalStr { get; set; }

        public decimal BankValue { get; set; }
        public decimal CallValue { get; set; }
        public decimal RaiseValue { get; set; }
        public decimal TotalValue { get; set; }

        public Image BankImg { get; set; }
        public Image CallImg { get; set; }
        public Image RaiseImg { get; set; }
        public Image TotalImg { get; set; }

        public bool CallValueValid { get; set; }
        public bool RaiseValueValid { get; set; }
        public bool TotalValueValid { get; set; }
        public bool BankValueValid { get; set; }

        public void Parse(bool removeDollar)
        {
            CallValueValid = false;
            RaiseValueValid = false;
            TotalValueValid = false;
            BankValueValid = false;
            decimal b;
            decimal c;
            decimal r;
            decimal t;

            if (removeDollar && !string.IsNullOrEmpty(TotalStr))
            {
                TotalStr = TotalStr.Trim().Trim(new char[] { '\n' });
                if (TotalStr.Length >= 1)
                    TotalStr = TotalStr.Substring(1);
            }

            if (removeDollar && !string.IsNullOrEmpty(BankStr))
            {
                BankStr = BankStr.Trim().Trim(new char[] { '\n' });
                if (BankStr.Length >= 1)
                    BankStr = BankStr.Substring(1);
            }

            if (removeDollar && !string.IsNullOrEmpty(CallStr))
            {
                CallStr = CallStr.Trim().Trim(new char[] { '\n' });
                if (CallStr.Length >= 1)
                    CallStr = CallStr.Substring(1);
            }

            if (removeDollar && !string.IsNullOrEmpty(RaiseStr))
            {
                RaiseStr = RaiseStr.Trim().Trim(new char[] { '\n' });
                if (RaiseStr.Length >= 1)
                    RaiseStr = RaiseStr.Substring(1);
            }

            var comma = '\u201A';

            var bstr = BankStr.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(bstr,
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out b))
            {
                BankValue = b;
                BankValueValid = true;
            }
            
            var cstr = CallStr.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ",string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(cstr,
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out c))
            {
                CallValue = c;
                CallValueValid = true;
            }

            var rstr = RaiseStr.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(rstr, 
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out r))
            {
                RaiseValue = r;
                RaiseValueValid = true;
            }

            var tstr = TotalStr.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(tstr,
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out t))
            {
                TotalValue = t;
                TotalValueValid = true;
            }
        }
    }
}