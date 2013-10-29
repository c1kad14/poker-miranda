using System;
using System.Drawing;

namespace miranda.ui
{
    public class PlayerBet
    {
        public PlayerBet()
        {
            Action = PlayerAction.Init;
        }
        //Variables
        public decimal CurrentBet { get; set; }
        public string Txt { get; set; }
        
        public Rectangle Rect { get; set; }
        public PlayerAction Action { get; set; }
        public Bitmap Image { get; set; }

        
        public bool IsValid { get; private set; }

        public bool IsMe { get; set; }

        public void Parse(bool removeDollar)
        {
            decimal d;
            
            if (Txt == "none")
            {
                CurrentBet = 0;
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

            if (decimal.TryParse(Txt.Trim().Trim(new char[]{'\n'}), out d))
            {
                CurrentBet = d;
                Action = PlayerAction.Init;
                IsValid = true;
            }
            else
            {
                CurrentBet = 0;
                Action = PlayerAction.Init;
                IsValid = false;
            }
        }

        public void AnalyseBets(decimal minBet, decimal lastMaxBet)
        {
            if (CurrentBet == 0)
            {
                Action = PlayerAction.Check;
                return;
            }

            var diff = CurrentBet - minBet;

            if (CurrentBet == lastMaxBet)
            {
                Action = PlayerAction.Call;
            }
            else if (diff > 0)
            {
                Action = PlayerAction.Raise;
            }
            else if (diff == 0 || diff == -(minBet / (decimal)2.0))
            {
                Action = PlayerAction.Call;
            }
            else
            {
                Action = PlayerAction.Init;
            }
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
            return str + "-" + CurrentBet + "-" + (string.IsNullOrEmpty(Txt) ? "" : Txt.Trim().Trim(new char[] { '\n' }));
        }

       
    }
}