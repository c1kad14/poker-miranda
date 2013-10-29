using System;
using System.Drawing;
using System.Globalization;

namespace miranda.ui
{
    /// <summary>
    /// Suit enumeration
    /// </summary>
    public enum PlayerAction
    {
        Init = 0,
        Check,
        Raise,
        Call,
        Fold
    }
    /// <summary>
    /// Class represents recognized playing card. Contains properties of card such as ; 
    /// Rank of card, suit of card, image of card which is extracted from source image ,
    /// corner points of card on source image
    /// </summary>
    public class Player
    {
        public Player()
        {
            Action = PlayerAction.Init;
            Stat = new PlayerStat();
        }
        //Variables
        public decimal Posa { get; set; }
        public decimal PosaPrev { get; set; }
        public string Txt { get; set; }

        public bool IsAllIn { get; set; }
        public bool IsFold { get; set; }

        public Rectangle Rect { get; set; }
        public PlayerAction Action { get; set; }
        public Bitmap Image { get; set; }

        public bool IsButton { get; set; }
        public bool IsValid { get; private set; }
        public bool IsMe { get; set; }

        public PlayerStat Stat { get; set; }

        

        public void Parse(bool removeDollar)
        {
            decimal d;
            
            if (Txt == "none")
            {
                Posa = 0;
                Action = PlayerAction.Init;
                IsValid = false;
                return;

            }
            if (removeDollar && !string.IsNullOrEmpty(Txt))
            {
                Txt = Txt.Trim().Trim(new char[] { '\n' });
                if(Txt.Length >= 1)
                    Txt = Txt.Substring(1);
            }
            var comma = '\u201A';
            var rstr = Txt.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(rstr,
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out d))
            {
                Posa = d;
                Action = PlayerAction.Init;
                IsValid = true;
            }
            else
            {
                Posa = 0;
                Action = PlayerAction.Init;
                IsValid = false;
            }
        }

        public void ParsePrev(bool removeDollar)
        {
            decimal d;

            if (Txt == "none")
            {
                PosaPrev = 0;
                Action = PlayerAction.Init;
                IsValid = false;
                return;

            }
            if (removeDollar && !string.IsNullOrEmpty(Txt))
            {
                Txt = Txt.Trim().Trim(new char[] { '\n' });
                if (Txt.Length >= 1)
                    Txt = Txt.Substring(1);
            }

            var comma = '\u201A';
            var rstr = 
                string.IsNullOrEmpty(Txt) ? "" :
                Txt.Trim().Trim(new char[] { '\n' }).Replace(comma, ',').Replace(',', '.')
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);
            if (decimal.TryParse(rstr,
                System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out d))
            {
                PosaPrev = d;
                Action = PlayerAction.Init;
                IsValid = true;
            }
            else
            {
                PosaPrev = 0;
                Action = PlayerAction.Init;
                IsValid = false;
            }
        }

        public decimal AnalyseBets(decimal minBet)
        {
            if (IsFold)
            {
                Action = PlayerAction.Fold;
                return 0;
            }
            var diff = PosaPrev - Posa;
            if (diff > minBet)
            {
                Action = PlayerAction.Raise;
            }
            else if (diff == 0)
            {
                Action = PlayerAction.Check;
            }
            else if (diff == minBet || diff <= (minBet / (decimal)2.0))
            {
                Action = PlayerAction.Call;
            }
            else
            {
                Action = PlayerAction.Init;
            }
            return diff;
        }

        public string GetAction
        {
            get
            {
               var str = "";
            switch (Action)
            {
                case PlayerAction.Init:
                    str = "Init";
                    break;
                case PlayerAction.Check:
                    str = "Check";
                    break;
                case PlayerAction.Raise:
                    str = "Raise";
                    break;
                case PlayerAction.Call:
                    str = "Call";
                    break;
                case PlayerAction.Fold:
                    str = "Fold";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return str;
            }
        }
        /// <summary>
        /// Overrided ToString Function.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            var str = "";
            switch (Action)
            {
                case PlayerAction.Init:
                    str = "Init";
                    break;
                case PlayerAction.Check:
                    str = "Check";
                    break;
                case PlayerAction.Raise:
                    str = "Raise";
                    break;
                case PlayerAction.Call:
                    str = "Call";
                    break;
                case PlayerAction.Fold:
                    str = "Fold";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return str + "-" + Posa + "-" + PosaPrevLog + "-" + (string.IsNullOrEmpty(Txt) ? "" : Txt.Trim().Trim(new char[] { '\n' })) + "-" + IsAllIn;
        }

        public bool IsActive
        {
            get { return IsValid; }
        }

        public decimal PosaPrevLog { get; set; }
    }
}
