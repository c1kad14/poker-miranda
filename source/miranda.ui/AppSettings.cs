using System.Drawing;

namespace miranda.ui
{
    public class AppSettings
    {
        public string Key { get; set; }

        public Rectangle AllCardsRect { get; set; }
        public Rectangle MyCardsRect { get; set; }

        public string TemplateFile { get; set; }
        public string TestFolder { get; set; }
        public int TimerInterval { get; set; }
        public Point WindowPoint { get; set; }

        public Rectangle TableRect { get; set; }
        public string TemplatePlayersFile { get; set; }

        public decimal MinBet { get; set; }
        public decimal WaitRoundLimit { get; set; }

        public Rectangle Player1 { get; set; }
        public Rectangle Player2 { get; set; }
        public Rectangle Player3 { get; set; }
        public Rectangle Player4 { get; set; }
        public Rectangle Player5 { get; set; }
        public Rectangle Player6 { get; set; }
        public Rectangle Player7 { get; set; }
        public Rectangle Player8 { get; set; }
        public Rectangle Player9 { get; set; }

        

        public decimal PreFlopWaitRoundLimit { get; set; }

        public Rectangle BalanceRect { get; set; }

        public Rectangle BankSizeRect { get; set; }

        public Rectangle CallRateRect { get; set; }

        public Rectangle Button1 { get; set; }
        public Rectangle Button2 { get; set; }
        public Rectangle Button3 { get; set; }
        public Rectangle Button4 { get; set; }
        public Rectangle Button5 { get; set; }
        public Rectangle Button6 { get; set; }
        public Rectangle Button7 { get; set; }
        public Rectangle Button8 { get; set; }
        public Rectangle Button9 { get; set; }

        public Rectangle ShortButtonRect1 { get; set; }
        public Rectangle ShortButtonRect2 { get; set; }
        public Rectangle ShortButtonRect3 { get; set; }
        public Rectangle ShortButtonRect4 { get; set; }

        public bool RemoveDollar { get; set; }

        public Rectangle BetInputRect { get; set; }

        public decimal BuyIn { get; set; }

        public Rectangle FoldRect { get; set; }
        public Rectangle CheckCallRect { get; set; }
        public Rectangle BetRaiseRect { get; set; }

        public Rectangle Bet1 { get; set; }
        public Rectangle Bet2 { get; set; }
        public Rectangle Bet3 { get; set; }
        public Rectangle Bet4 { get; set; }
        public Rectangle Bet5 { get; set; }
        public Rectangle Bet6 { get; set; }

        public Rectangle RaiseRateRect { get; set; }

        public Rectangle Fold1 { get; set; }

        public Rectangle Fold2 { get; set; }

        public Rectangle Fold3 { get; set; }

        public Rectangle Fold4 { get; set; }

        public Rectangle Fold5 { get; set; }

        public Rectangle Fold6 { get; set; }

        public Rectangle RandomTableRect { get; set; }

        public Rectangle PauseRect { get; set; }

        public Rectangle ResumeRect { get; set; }

        public Rectangle Fold7 { get; set; }

        public Rectangle Fold8 { get; set; }

        public Rectangle Fold9 { get; set; }

        public decimal Limit { get; set; }

        public Rectangle RectOpenTable { get; set; }
        public Rectangle RectCancelTable { get; set; }
        public Rectangle RectBuyInOk { get; set; }


        public Rectangle RectTableX { get; set; }
        public Rectangle RectLeaveTableOk { get; set; }

        public decimal RebuyLevel { get; set; }

        public Rectangle CardSuitRect { get; set; }

        public Rectangle CardRankRect { get; set; }

        public decimal MinCardSize { get; set; }

        public Rectangle TxtResultRect { get; set; }

        public Rectangle TotalSizeRect { get; set; }

        public decimal BluffPercent { get; set; }

        public decimal ShortStackPercent { get; set; }
    }
}