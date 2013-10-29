using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
using System.Linq;
using miranda.ui.Properties;
using System.Threading;
using LogLoader;

namespace miranda.ui
{
    public class Table
    {
        public int TotalRounds { get; set; }
        public int ButtonPlayer
        {
            get
            {
                for (int i = 0; i < DealerButtons.Count; i++)
                {
                    if (DealerButtons[i])
                        return i;
                }
                return -1;
            }
        }

        public int ButtonPlayerPrev { get; set; }

        Rectangle CardSuitRect;
        Rectangle CardRankRect;
        int CardMinSize;
        public Table(int playerCount, AppSettings settings)
        {
            TotalRounds = 0;

            ButtonPlayerPrev = -999;
            _random = new Random();
            ShortButton1 = settings.ShortButtonRect1;
            ShortButton2 = settings.ShortButtonRect2;
            ShortButton3 = settings.ShortButtonRect3;
            ShortButton4 = settings.ShortButtonRect4;

            CardSuitRect = settings.CardSuitRect;
            CardRankRect = settings.CardRankRect;
            CardMinSize = (int)settings.MinCardSize;

            BetCounterPreFlop = 1;
            BetCounterFlop = 1;
            BetCounterTurn = 1;
            BetCounterRiver = 1;
            RaiseCounter = 0;

            PlayerActionCounter = 0;

            PlayerRects = new List<Rectangle>();
            PlayerFolds = new List<Rectangle>();
            ButtonRects = new List<Rectangle>();
            BetRects = new List<Rectangle>();

            Players = new PlayerCollection();
            DealerButtons = new List<bool>();
            PlayerBets = new List<PlayerBet>();

            MyBalance = new Player();
            Bank = new Bank();

            for (int i = 0; i < playerCount; i++)
            {
                Players.Add(new Player {Action = PlayerAction.Init});
            }

            for (int i = 0; i < playerCount; i++)
            {
                DealerButtons.Add(false);
            }

            for (int i = 0; i < playerCount; i++)
            {
                PlayerBets.Add(new PlayerBet());
            }

            _cardRecognizer = new CardRecognizer();
            
        }

        public List<PlayerBet> PlayerBets { get; private set; }

        public decimal MinBet { get; set; }
        public string TempDir { get; set; }

        public Point WindowPoint { get; set; }
        
        public Bitmap MyCardsBmp { get; set; }
        public Bitmap AllCardsBmp { get; set; }
        public Bitmap TableBmp { get; set; }


        public PlayerCollection Players { get; /*private*/ set; }

        public List<bool> DealerButtons { get; set; } 

        
        public Player MyBalance { get; private set; }
        public Bank Bank { get; private set; }

        public List<Rectangle> PlayerRects { get; set; }
        public List<Rectangle> PlayerFolds { get; set; }
        //public Rectangle Player2 { get; set; }
        //public Rectangle Player3 { get; set; }
        //public Rectangle Player4 { get; set; }
        //public Rectangle Player5 { get; set; }
        //public Rectangle Player6 { get; set; }
        //public Rectangle Player7 { get; set; }
        //public Rectangle Player8 { get; set; }
        //public Rectangle Player9 { get; set; }

        private CardRecognizer _cardRecognizer;

        public int PlayerActionCounter { get; set; }
        public int RaiseCounter { get; set; }
        public int BetCounterPreFlop { get; set; }
        public int BetCounterFlop { get; set; }
        public int BetCounterTurn { get; set; }
        public int BetCounterRiver { get; set; }

        public delegate void ClickLogDelegate(string msg);

        public event ClickLogDelegate ClickLog;
        public event ClickLogDelegate AfterClickLog;
        public event ClickLogDelegate ClickLogDebug;

        public enum TablePosition
        {
            Unknown = 0,
            Blind = 1,
            Middle,
            Button,
            CutOff,
            Early,
            SmallBlind
        }

        

        public bool IsMiddle
        {
            get { return MyPosition == TablePosition.Middle; }
        }
        public bool IsButton
        {
            get { return MyPosition == TablePosition.Button; }
        }

        public bool IsAllBlind
        {
            get
            {
                return IsBlind || IsSmallBlind;
            }
        }
        public bool IsBlind
        {
            get 
            { 
                return MyPosition == TablePosition.Blind; 
            }
        }

        public bool IsSmallBlind
        {
            get
            {
                return MyPosition == TablePosition.SmallBlind;
            }
        }

        public bool IsCutOff
        {
            get
            {
                return MyPosition == TablePosition.CutOff;
            }
        }

        public bool IsEarly
        {
            get
            {
                return MyPosition == TablePosition.Early;
            }
        }

        public bool IsButtonOrCutOff
        {
            get
            {
                return IsButton || IsCutOff;
            }
        }

        public bool IsLate 
        {
            get
            {
                return IsButton || IsCutOff || IsAllBlind;
            }
        }

        //TODO 3-bet chart
        public TablePosition LastRaisedPosition
        {
            get
            {
                if (DealerButtons.Count == 0)
                {
                    return TablePosition.Unknown;
                }


                var newButtons = new List<bool>(DealerButtons);
                var newPlayers = new List<Player>(Players);

                int i = 0;
                while (i < newPlayers.Count)
                {
                    if (!newPlayers[i].IsActive && !newPlayers[i].IsAllIn)
                    {
                        newPlayers.RemoveAt(i);
                        newButtons.RemoveAt(i);
                    }
                    else
                        i++;
                }

                var idx = newPlayers.FindLastIndex(player => player.Action == PlayerAction.Raise);
                if (idx != -1)
                {
                    
                    int buttonPos = -1;
                    for (int j = 0; j < newButtons.Count; j++)
                    {
                        if (newButtons[j])
                        {
                            buttonPos = j;
                            break;
                        }
                    }

                    if (buttonPos != -1)
                    {
                        
                        if (idx == buttonPos) return TablePosition.Button;
                        if (idx - buttonPos <= 2) return TablePosition.Blind;

                        if (buttonPos - idx == 1) return TablePosition.CutOff;

                        if (newButtons.Count == 4 && idx - buttonPos == 3) return TablePosition.CutOff; 
                        if (newButtons.Count > 4 && idx - buttonPos >= 3) return TablePosition.Early;

                        return TablePosition.Middle;
                    }
                }
                return TablePosition.Unknown;
            }
        }
        public TablePosition MyPosition
        {
            get
            {
                if (DealerButtons.Count == 0)
                {
                    return TablePosition.Unknown;
                }

                if(DealerButtons[DealerButtons.Count - 1])
                    return TablePosition.Button;
                
                var newButtons = new List<bool>(DealerButtons);
                var newPlayers = new List<Player>(Players);

                int i = 0;
                while (i < newPlayers.Count)
                {
                    if (!newPlayers[i].IsActive && !newPlayers[i].IsAllIn)
                    {
                        newPlayers.RemoveAt(i);
                        newButtons.RemoveAt(i);
                    }
                    else
                        i++;
                }

                
                

                if (newButtons.Count == 2)
                {

                    return TablePosition.Blind;
                }

                if (newButtons.Count >= 3)
                {
                    
                    if(newButtons[newButtons.Count - 2])
                        return TablePosition.SmallBlind;

                    if (newButtons[newButtons.Count - 3])
                        return TablePosition.Blind;
                }

                if (newButtons.Count >= 4 && newButtons[0])
                {
                    return TablePosition.CutOff;
                }

                if (newButtons.Count > 4 && newButtons[newButtons.Count - 4])
                {
                    return TablePosition.Early;
                }

                return TablePosition.Middle;
            }
        }
        public int PlayersRaisedCount
        {
            get
            {
                var pl = Players.FindAll(player => (player.Action == PlayerAction.Raise || player.IsAllIn));
                return pl.Count;
            }
        }

        public Rectangle BankRect { get; set; }
        public Rectangle RandomTableRect { get; set; }

        public void RecognizeFromWholeBmp(Bitmap bmp, AppSettings settings)
        {
            for (int i = 0; i < PlayerRects.Count; i++)
            {
                var playerRect = PlayerRects[i];
                if (playerRect == Rectangle.Empty)
                    continue;

                var bmpPl = GetArea(bmp, playerRect);
                var txt = _cardRecognizer.RecognizeText(bmpPl);

                Players[i].Txt = txt;
                Players[i].Image = bmpPl;
                Players[i].Parse(settings.RemoveDollar);

                if (PlayerFolds[i] != Rectangle.Empty)
                {
                    var bmpFl = GetArea(bmp, PlayerFolds[i]);
                    Players[i].IsFold = !_cardRecognizer.PlayerHasCards(bmpFl);
                }
            }

            Players[Players.Count - 1].IsMe = true;

            for (int i = 0; i < DealerButtons.Count; i++)
            {
                DealerButtons[i] = false;
            }

            for (int i = 0; i < ButtonRects.Count; i++)
            {
                var buttonRect = ButtonRects[i];
                if (buttonRect == Rectangle.Empty)
                    continue;

                var bmpPl = GetArea(bmp, buttonRect);

                if (_cardRecognizer.IsButton(bmpPl, null))
                    DealerButtons[i] = true;
            }

            //TODO player bets
            //decimal lastMaxBet = 0;
            //for (int i = 0; i < BetRects.Count; i++)
            //{
            //    var betRect = BetRects[i];
            //    if (betRect == Rectangle.Empty)
            //        continue;

            //    var bmpPl = GetArea(bmp, betRect);
            //    var txt = _cardRecognizer.RecognizeText(bmpPl);

            //    txt = RecognizeBet(betRect, bmp, 23);

            //    int cnt = 0;
            //    while ((txt == "" || txt == "none") && cnt < 3)
            //    {
            //        txt = RecognizeBet(betRect, bmp, 23*i);
            //        cnt++;
            //    }
                
            //    cnt = 0;
            //    while ((txt == "" || txt == "none") && cnt < 3)
            //    {
            //        txt = RecognizeBet(betRect, bmp, - 23 * i);
            //        cnt++;
            //    }

            //    PlayerBets[i].Txt = txt;
            //    PlayerBets[i].Image = bmpPl;
            //    PlayerBets[i].Parse(settings.RemoveDollar);
                
            //    //PlayerBets[i].AnalyseBets(settings.MinBet, lastMaxBet);
            //    //if (lastMaxBet < PlayerBets[i].CurrentBet)
            //    //    lastMaxBet = PlayerBets[i].CurrentBet;
            //}

            //PlayerBets[PlayerBets.Count - 1].IsMe = true;

            if (settings.BalanceRect != Rectangle.Empty)
            {
                var bmpMy = GetArea(bmp, settings.BalanceRect);
                MyBalance.Txt = _cardRecognizer.RecognizeText(bmpMy);
                MyBalance.Image = bmpMy;
                MyBalance.Parse(settings.RemoveDollar);
            }

            RandomTableRect = settings.RandomTableRect;
            PauseRect = settings.PauseRect;
            ResumeRect = settings.ResumeRect;
            BuyIn = settings.BuyIn;

            RectBuyInOk = settings.RectBuyInOk;
            RectCancelTable = settings.RectCancelTable;
            RectLeaveTableOk = settings.RectLeaveTableOk;
            RectOpenTable = settings.RectOpenTable;
            RectTableX = settings.RectTableX;

            if (settings.BankSizeRect != Rectangle.Empty)
            {
                var bmpBank = GetArea(bmp, settings.BankSizeRect);
                Bank.BankStr = _cardRecognizer.RecognizeText(bmpBank);
                Bank.BankImg = bmpBank;
                BankRect = settings.BankSizeRect;
            }

            if (settings.CallRateRect != Rectangle.Empty)
            {
                var bmpCall = GetArea(bmp, settings.CallRateRect);
                Bank.CallStr = _cardRecognizer.RecognizeTextSmall(bmpCall);
                Bank.CallImg = bmpCall;
            }

            if (settings.RaiseRateRect != Rectangle.Empty)
            {
                var bmpRaise = GetArea(bmp, settings.RaiseRateRect);
                Bank.RaiseStr = _cardRecognizer.RecognizeTextSmall(bmpRaise);
                Bank.RaiseImg = bmpRaise;
            }

            Bank.Parse(settings.RemoveDollar);

            MyCardsBmp = GetArea(bmp, settings.MyCardsRect);
            AllCardsBmp = GetArea(bmp, settings.AllCardsRect);
            TableBmp = bmp;

            try
            {
                MyCards = _cardRecognizer.Recognize(MyCardsBmp, TempDir, 0, CardMinSize, CardSuitRect, CardRankRect);
            }
            catch (Exception ex)
            {
                MyCards = new CardCollection();
            }

            try
            {
                AllCards = _cardRecognizer.Recognize(AllCardsBmp, TempDir, 0, CardMinSize, CardSuitRect, CardRankRect);
            }
            catch (Exception ex)
            {
                AllCards = new CardCollection();
            }

            try
            {
                Buttons = new ButtonCollection();
                
                ButtonFoldBmp = GetArea(bmp, settings.FoldRect);
                ButtonCallCheckBmp = GetArea(bmp, settings.CheckCallRect);
                ButtonBetRaiseBmp = GetArea(bmp, settings.BetRaiseRect);

                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonFoldBmp, settings.FoldRect, TempDir, 0));
                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonCallCheckBmp, settings.CheckCallRect, TempDir, 0));
                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonBetRaiseBmp, settings.BetRaiseRect, TempDir, 0));
            }
            catch (Exception ex)
            {
                Buttons = new ButtonCollection();
            }


            //var txtRestImg = GetArea(bmp, settings.TxtResultRect);
            //TxtResult = _cardRecognizer.RecognizeText2(txtRestImg, "", 0);

        }

        private string RecognizeBet(Rectangle betRect, Bitmap bmp, int add)
        {
            betRect = new Rectangle(betRect.X + add, betRect.Y, betRect.Width, betRect.Height);
            var bmpPl = GetArea(bmp, betRect);
            return _cardRecognizer.RecognizeText(bmpPl);
        }


        public CardCollection RecogniseCard(Bitmap bmp)
        {
            var cards = new CardCollection();
            
            try
            {
                cards = _cardRecognizer.Recognize(bmp, TempDir, 0, CardMinSize, CardSuitRect, CardRankRect);
            }
            catch (Exception ex)
            {
                
            }
            return cards;
        }


        public Bitmap GetAreaFromScreen(Rectangle area)
        {
            Thread.Sleep(0);

            var rect = new Rectangle(WindowPoint.X + area.X, WindowPoint.Y + area.Y, area.Width, area.Height);

            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            ;

            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp;
        }
        public void RecogniseButton(AppSettings settings)
        {
            
            try
            {
                Buttons = new ButtonCollection();
                
                ButtonFoldBmp = GetAreaFromScreen(settings.FoldRect);
                ButtonCallCheckBmp = GetAreaFromScreen(settings.CheckCallRect);
                ButtonBetRaiseBmp = GetAreaFromScreen(settings.BetRaiseRect);

                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonFoldBmp, settings.FoldRect, TempDir, 0));
                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonCallCheckBmp, settings.CheckCallRect, TempDir, 0));
                Buttons.Add(_cardRecognizer.RecognizeOneButton(ButtonBetRaiseBmp, settings.BetRaiseRect, TempDir, 0));
            }
            catch (Exception ex)
            {
                
            }
        }

        private bool _parseCounter = false;

        public void RecognizeFromWin(AppSettings settings)
        {
            for (int i = 0; i < PlayerRects.Count; i++)
            {
                var playerRect = PlayerRects[i];

                if(playerRect == Rectangle.Empty)
                    continue;

                var bmpPl = GetAreaFromScreen(playerRect);
                
                var txt = _cardRecognizer.RecognizeText(bmpPl);

                Players[i].Txt = txt;
                Players[i].Image = bmpPl;

                if (PlayerFolds[i] != Rectangle.Empty)
                {
                    var bmpFl = GetAreaFromScreen(PlayerFolds[i]);
                    Players[i].IsFold = !_cardRecognizer.PlayerHasCards(bmpFl);
                }
            }

            //Ex.Report(new Exception(Players[0].Txt));

            Players[Players.Count - 1].IsMe = true;

            for (int i = 0; i < DealerButtons.Count; i++)
            {
                DealerButtons[i] = false;
            }

            for (int i = 0; i < ButtonRects.Count; i++)
            {
                var buttonRect = ButtonRects[i];
                if (buttonRect == Rectangle.Empty)
                    continue;

                var bmpPl = GetAreaFromScreen(buttonRect);

                if (_cardRecognizer.IsButton(bmpPl, null))
                    DealerButtons[i] = true;
            }


            //decimal lastMaxBet = 0;
            //for (int i = 0; i < BetRects.Count; i++)
            //{
            //    var betRect = BetRects[i];
            //    if (betRect == Rectangle.Empty)
            //        continue;

            //    var bmpPl = GetAreaFromScreen(betRect);
            //    var txt = _cardRecognizer.RecognizeText(bmpPl);

            //    PlayerBets[i].Txt = txt;
            //    PlayerBets[i].Image = bmpPl;
            //    PlayerBets[i].Parse(settings.RemoveDollar);
            //    PlayerBets[i].AnalyseBets(settings.MinBet, lastMaxBet);

            //    if (lastMaxBet < PlayerBets[i].CurrentBet)
            //        lastMaxBet = PlayerBets[i].CurrentBet;
            //}

            //PlayerBets[PlayerBets.Count - 1].IsMe = true;

            //TODO slower than by area
            //TableBmp = GetAreaFromScreen(TableArea);

            //MyCardsBmp = GetArea(TableBmp, MyCardsArea);
            //AllCardsBmp = GetArea(TableBmp, AllCardsArea);
            //ButtonsBmp = GetArea(TableBmp, ButtonsArea);

            if (settings.BalanceRect != Rectangle.Empty)
            {
                var bmpMy = GetAreaFromScreen(settings.BalanceRect);
                MyBalance.Txt = _cardRecognizer.RecognizeText(bmpMy);
                MyBalance.Image = bmpMy;
                MyBalance.Parse(settings.RemoveDollar);
            }

            RandomTableRect = settings.RandomTableRect;
            PauseRect = settings.PauseRect;
            ResumeRect = settings.ResumeRect;
            BuyIn = settings.BuyIn;

            RectBuyInOk = settings.RectBuyInOk;
            RectCancelTable = settings.RectCancelTable;
            RectLeaveTableOk = settings.RectLeaveTableOk;
            RectOpenTable = settings.RectOpenTable;
            RectTableX = settings.RectTableX;


            if (settings.TotalSizeRect != Rectangle.Empty)
            {
                var bmpTotal = GetAreaFromScreen(settings.TotalSizeRect);
                Bank.TotalStr = _cardRecognizer.RecognizeText(bmpTotal);
                Bank.TotalImg = bmpTotal;
            }

            if (settings.BankSizeRect != Rectangle.Empty)
            {
                var bmpBank = GetAreaFromScreen(settings.BankSizeRect);
                Bank.BankStr = _cardRecognizer.RecognizeText(bmpBank);
                Bank.BankImg = bmpBank;
                BankRect = settings.BankSizeRect;
            }

            if (settings.CallRateRect != Rectangle.Empty)
            {
                var bmpCall = GetAreaFromScreen(settings.CallRateRect);
                Bank.CallStr = _cardRecognizer.RecognizeTextSmall(bmpCall);
                Bank.CallImg = bmpCall;
            }

            if (settings.RaiseRateRect != Rectangle.Empty)
            {
                var bmpRaise = GetAreaFromScreen(settings.RaiseRateRect);
                Bank.RaiseStr = _cardRecognizer.RecognizeTextSmall(bmpRaise);
                Bank.RaiseImg = bmpRaise;
            }
            Bank.Parse(settings.RemoveDollar);

            MyCardsBmp = GetAreaFromScreen(settings.MyCardsRect);
            AllCardsBmp = GetAreaFromScreen(settings.AllCardsRect);

            var prevAllCards = AllCards;

            MyCards = RecogniseCard(MyCardsBmp);
            AllCards = RecogniseCard(AllCardsBmp);
            RecogniseButton(settings);

        }

        public void ClearTable()
        {
            MyCardsBmp = null;
            AllCardsBmp = null;
            TableBmp = null;

            MyCards = null;
            AllCards = null;
            Buttons = null;


            Bank.CallImg = null;
            Bank.CallStr = "";
            Bank.CallValue = 0;
            Bank.CallValueValid = false;

            Bank.BankImg = null;
            Bank.BankStr = "";
            Bank.BankValue = 0;
            

            Bank.RaiseImg = null;
            Bank.RaiseStr = "";
            Bank.RaiseValue = 0;
            Bank.RaiseValueValid = false;
        }

        public CardCollection MyCards { get; set; }
        public CardCollection AllCards { get; set; }
        public ButtonCollection Buttons { get; set; }

        public Rectangle ShortButton1 { get; set; }
        public Rectangle ShortButton2 { get; set; }
        public Rectangle ShortButton3 { get; set; }
        public Rectangle ShortButton4 { get; set; }

        public bool NoRaise
        {
            get
            {
                if (
                    //[fold] [call 0.02] [raise 0.04]
                    (
                    Bank.CallValueValid && Bank.CallValue <= MinBet
                    &&
                    Bank.RaiseValueValid && Bank.RaiseValue <= MinBet * 2
                    )
                    ||
                    //[fold] [call 0] [raise 0.04]
                    Bank.RaiseValueValid && Bank.RaiseValue <= MinBet*2
                    ||
                    //[fold] [check] [bet 0.02]
                    Bank.RaiseValueValid && Bank.RaiseValue <= MinBet
                    )
                    return true;

                return false;

            }
        }

        public bool IsValid
        {
            get 
            { 
                var valid = true;

                valid &= MyCards.Count == 2;
                valid &= Buttons.Count > 0;
           
                bool hasFold = false;
                bool hasCheck = false;
                bool hasRaise = false;
                bool hasCall = false;
                bool hasBet = false;
                foreach (Button button in Buttons)
                {
                    switch (button.Tip)
                    {
                        case ButtonTip.NOT_RECOGNIZED:
                            break;
                        case ButtonTip.Fold:
                            hasFold = true;
                            break;
                        case ButtonTip.Check:
                            hasCheck = true;
                            break;
                        case ButtonTip.Raise:
                            hasRaise = true;
                            break;
                        case ButtonTip.Call:
                            hasCall = true;
                            break;
                        case ButtonTip.Bet:
                            hasBet = true;
                            break;
                        default:
                            break;
                    }
                }
                valid &= (hasBet | hasCall | hasCheck | hasFold | hasRaise);

                return valid;
            }
        }

        public bool IsValidButtons
        {
            get
            {
                var valid = true;

                valid &= Buttons.Count > 0;

                bool hasFold = false;
                bool hasCheck = false;
                bool hasRaise = false;
                bool hasCall = false;
                bool hasBet = false;
                foreach (Button button in Buttons)
                {
                    switch (button.Tip)
                    {
                        case ButtonTip.NOT_RECOGNIZED:
                            break;
                        case ButtonTip.Fold:
                            hasFold = true;
                            break;
                        case ButtonTip.Check:
                            hasCheck = true;
                            break;
                        case ButtonTip.Raise:
                            hasRaise = true;
                            break;
                        case ButtonTip.Call:
                            hasCall = true;
                            break;
                        case ButtonTip.Bet:
                            hasBet = true;
                            break;
                        default:
                            break;
                    }
                }
                valid &= (hasBet | hasCall | hasCheck | hasFold | hasRaise);

                return valid;
            }
        }

        public bool IsPreFlop
        {
            get { return AllCards.Count == 0; }
        }

        public bool IsFlop
        {
            get { return AllCards.Count == 3; }
        }

        public bool IsTurn
        {
            get { return AllCards.Count == 4; }
        }

        public bool IsRiver
        {
            get { return AllCards.Count == 5; }
        }

        private const int Add = 20;

        public bool ClickFold()
        {
            var res = Click(ButtonTip.Fold, false);
            if (res)
            {
                BetCounterPreFlop = 1;
                BetCounterFlop = 1;
                BetCounterTurn = 1;
                BetCounterRiver = 1;
                RaiseCounter = 0;
            }
            return res;
        }

        /// <summary>
        /// Use ONLY after flop (flop, turn, river)
        /// </summary>
        public bool IsFreePlay { get; set; }

        public decimal BuyIn { get; set; }

        //TODO useless  limit validation
        //bool ValidateLimit(ButtonTip tip)
        //{
        //    if (tip == ButtonTip.Call || tip == ButtonTip.Raise || tip == ButtonTip.Bet)
        //    {
        //        //if (!MyBalance.IsValid)
        //        //    return false;

        //        //impossible
        //        if (Bank.CallValueValid && !Bank.RaiseValueValid)
        //        {
        //            if (Bank.CallValue <= BuyIn)
        //                return true;

        //            return false;
        //        }

        //        //bet or all-in
        //        if (!Bank.CallValueValid && Bank.RaiseValueValid)
        //        {
        //            if (Bank.RaiseValue <= BuyIn)
        //                return true;

        //            return false;
        //        }

        //        //call & raise
        //        if (Bank.CallValueValid && Bank.RaiseValueValid)
        //        {
        //            if (tip == ButtonTip.Call)
        //            {
        //                if (Bank.CallValue <= BuyIn)
        //                    return true;

        //                return false;
        //            }
        //            if (tip == ButtonTip.Raise)
        //            {
        //                if (Bank.RaiseValue <= BuyIn)
        //                    return true;

        //                return false;
        //            }

        //            if (tip == ButtonTip.Bet)
        //            {
        //                if (Bank.RaiseValue <= BuyIn)
        //                    return true;

        //                return false;
        //            }

        //            return false;
        //        }
        //    }
        //    return true;
        //}
        bool Click(ButtonTip tip, bool validateLimit)
        {
            PlayerActionCounter += 1;

            //TODO useless  limit validation
            ////if (validateLimit)
            //{
            //    var lim = ValidateLimit(tip);

            //    if (!lim)
            //    {
            //        var msg = Algo + ":" + tip.ToString() + ";out of limit:" + MyBalance.Posa + ";" + Bank.CallValue + ";" + Bank.RaiseValue;
            //        Ex.Info(msg);
            //        if (ClickLogDebug != null)
            //        {
            //            ClickLogDebug(DateTime.Now.ToString("HH:mm:ss") + msg);
            //        }
            //        return false;
            //    }
            //}

            Button bt = null;
            foreach (Button button in Buttons)
            {
                if (button.Tip == tip)
                {
                    bt = button;
                    break;
                }
            }

            if (bt != null)
            {

                if ((bt.Tip == ButtonTip.Bet || bt.Tip == ButtonTip.Raise || bt.Tip == ButtonTip.Call))
                {
                    IsFreePlay = false;
                }

                //var rnd = new Random();
                var rect = bt.Rect;
                var rndValX = _random.Next(rect.Width / 6, rect.Width - rect.Width / 6);
                var rndValY = _random.Next(rect.Height / 6, rect.Height - rect.Height / 6);

                var x = WindowPoint.X + rect.X + rndValX;
                var y = WindowPoint.Y + rect.Y + rndValY;

                //var x = WindowPoint.X + rect.X + Add + rndValX;
                //var y = WindowPoint.Y + rect.Y + Add + rndValY;

                

                if (ClickLog != null)
                {
                    ClickLog(Algo + ":" + tip.ToString());
                }

                WinApi.ClickOnPoint(IntPtr.Zero, new Point(x, y));

                //User simulation
                if (RandomTableRect != Rectangle.Empty)
                {
                    //var rndXX = _random.Next(RandomTableRect.Width / 10, RandomTableRect.Width);

                    var rndXX = _random.Next(bt.Rect.Width / 10, bt.Rect.Width);

                    var rndYY = _random.Next(RandomTableRect.Height / 10, RandomTableRect.Height);

                    var x1 = WindowPoint.X + rndXX + bt.Rect.X;
                    var y1 = WindowPoint.Y + rndYY + RandomTableRect.Y;
                    WinApi.MoveOnPoint(IntPtr.Zero, new Point(x1, y1));
                }
                //End user simulation

                if (AfterClickLog != null)
                {
                    AfterClickLog(Algo + ":" + tip.ToString());
                }

                return true;
            }
            return false;
        }

        

        public string Algo { get; set; }
        public bool IsInitiative { get; set; }

        public List<Rectangle> ButtonRects { get; set; }
        public List<Rectangle> BetRects { get; set; }


        public int RaiserCount
        {
            get
            {
                return
                    Players.Count(player => !player.IsMe && player.IsActive && player.Action == PlayerAction.Raise);
            }
        }

        public bool IsRaiserCoSbBu
        {
            get
            {
                return RaiserPosition == TablePosition.CutOff 
                    || RaiserPosition == TablePosition.SmallBlind 
                    || RaiserPosition == TablePosition.Button;
            }
        }

        public bool IsRaiserMiddle
        {
            get { return RaiserPosition == TablePosition.Middle; }
        }

        public bool IsRaiserEarly
        {
            get { return RaiserPosition == TablePosition.Early; }
        }

        public void SetRaiserPosition()
        {
            RaiserPosition = GetRaiserPosition();
        }

        TablePosition GetRaiserPosition()
        {
            if (DealerButtons.Count == 0)
            {
                return TablePosition.Unknown;
            }


            var newButtons = new List<bool>(DealerButtons);
            var newPlayers = new List<Player>(Players);

            int i = 0;
            while (i < newPlayers.Count)
            {
                if (!newPlayers[i].IsActive && !newPlayers[i].IsAllIn)
                {
                    newPlayers.RemoveAt(i);
                    newButtons.RemoveAt(i);
                }
                else
                    i++;
            }

            if(newButtons.Count == 0)
                return TablePosition.Unknown;

            i = 0;
            int buttonIdx = -1;
            int round = 0;
            while (round < 2)
            {
                if (newButtons[i])
                    buttonIdx = i;

                var player = newPlayers[i];
                if (buttonIdx != -1 && player.Action == PlayerAction.Raise && !player.IsMe)
                {
                    if (i - buttonIdx == 0)
                        return TablePosition.Button;

                    if (i - buttonIdx == 1)
                        return TablePosition.SmallBlind;

                    if (i - buttonIdx == 2)
                        return TablePosition.Blind;

                    if (i - buttonIdx == 3 && newPlayers.Count >= 8)
                        return TablePosition.Early;

                    if (i - buttonIdx == -1)
                        return TablePosition.CutOff;
                }

                if (i == newButtons.Count - 1)
                {
                    i = 0;
                    round++;
                }
                else
                    i++;
            }
            return TablePosition.Middle;
        }
        public TablePosition RaiserPosition { get; private set; }
        /// <summary>
        /// Перед вами все сбрасывают карты.
        /// </summary>
        public bool DdAllCheckOrFold
        {
            get
            {
                return NoRaise;
                return Players.Count(
                        player =>
                        !player.IsMe &&
                        (player.Action == PlayerAction.Check 
                            || player.Action == PlayerAction.Init
                            || player.Action == PlayerAction.Fold
                            ))
                        == 
                        Players.Count(player => !player.IsMe)
                        ;
            }
        }

        /// <summary>
        /// Если кто-то повысил
        /// </summary>
        public bool DdAnyRaised
        {
            get
            {
                return !NoRaise;
                //|| Players.Exists(player => !player.IsMe && (player.Action == PlayerAction.Raise || player.IsAllIn));
            }
        }
        /// <summary>
        /// Один игрок уравнивает.
        /// Двое или больше игроков уравнивают.
        /// </summary>
        public bool DdAllCalled
        {
            get
            {
                return NoRaise;
                return Players.Exists(player => !player.IsMe &&
                    (player.Action == PlayerAction.Call 
                        || player.Action == PlayerAction.Init 
                        || player.Action == PlayerAction.Check))
                       &&
                       !Players.Exists(player => !player.IsMe && (player.Action == PlayerAction.Raise || player.IsAllIn))
                        ;
            }
        }

        /// <summary>
        /// Один игрок повышает, все остальные сбрасываются.
        /// </summary>
        public bool DdOneRaisedAllFold
        {
            get
            {
                return
                    Players.Count(player => !player.IsMe && (player.Action == PlayerAction.Raise || player.IsAllIn))
                    +
                    Players.Count(player => !player.IsMe && 
                        (
                            player.Action == PlayerAction.Fold
                            ||
                            player.Action == PlayerAction.Init
                        )
                    )
                    ==
                    Players.Count(player => !player.IsMe);
            }
        }

        /// <summary>
        /// Один игрок повышает, и, по меньшей мере, один игрок уравнивает.
        /// </summary>
        public bool DdOneRaisedOneCalled
        {
            get
            {
                return
                    Players.Count(player => !player.IsMe && player.Action == PlayerAction.Call) > 0
                    &&
                    Players.Count(player => !player.IsMe && (player.Action == PlayerAction.Raise || player.IsAllIn)) == 1;
            }
        }

        public bool ClickCall(bool validateLimit)
        {
            var res = Click(ButtonTip.Call, validateLimit);
            if (res)
            {
                if (IsPreFlop)
                    BetCounterPreFlop += 1;

                if (IsFlop)
                    BetCounterFlop += 1;

                if (IsTurn)
                    BetCounterTurn += 1;

                if (IsRiver)
                    BetCounterRiver += 1;

            }
            return res;
        }

        public bool ClickRaise(bool validateLimit)
        {
            var res = Click(ButtonTip.Raise, validateLimit);
            if (res)
            {
                if (IsPreFlop)
                    BetCounterPreFlop += 1;

                if (IsFlop)
                    BetCounterFlop += 1;

                if (IsTurn)
                    BetCounterTurn += 1;

                if (IsRiver)
                    BetCounterRiver += 1;
                
                RaiseCounter += 1;
               
            }
            return res;
        }

        public bool ClickBet(bool validateLimit)
        {
            var res = Click(ButtonTip.Bet, validateLimit);
            if (res)
            {
                if (IsPreFlop)
                    BetCounterPreFlop += 1;

                if (IsFlop)
                    BetCounterFlop += 1;

                if (IsTurn)
                    BetCounterTurn += 1;

                if (IsRiver)
                    BetCounterRiver += 1;

                RaiseCounter += 1;
            }
            return res;
        }

        private readonly Random _random;

        public bool ClickBet(decimal bet)
        {
            //var rnd = new Random();
            var add = _random.Next(-1, 1);
            var newBet = bet + MinBet*add;
            //ClickShort(BetInput, 0);
            
            MoveMouse(BetInput);
            DoubleClick(BetInput);

            if (ClickLog != null)
            {
                ClickLog(Algo + ":" + bet.ToString());
            }
            WinApi.TypeNumber(newBet);
            return true;
        }

        public bool ClickButton1()
        {
            return ClickShort(ShortButton1, 1);
        }
        public bool ClickButton2()
        {
            return ClickShort(ShortButton2, 2);
        }
        public bool ClickButton3()
        {
            return ClickShort(ShortButton3, 3);
        }
        public bool ClickButton4()
        {
            return ClickShort(ShortButton4, 4);
        }

        bool DoubleClick(Rectangle rect)
        {
            var rnd = new Random();
            var rndValX = rnd.Next(0, 2);
            var rndValY = rnd.Next(0, 2);

            var x = WindowPoint.X + rect.X + rect.Width / 2 + rndValX;
            var y = WindowPoint.Y + rect.Y + rect.Height / 2 + rndValY;

            WinApi.ClickOnPoint(IntPtr.Zero, new Point(x, y));
            Thread.Sleep(100);
            WinApi.ClickOnPoint(IntPtr.Zero, new Point(x, y));

            return true;
        }

        bool MoveMouse(Rectangle rect)
        {
            var rnd = new Random();
            var rndValX = rnd.Next(0, 2);
            var rndValY = rnd.Next(0, 2);

            var x = WindowPoint.X + rect.X + rect.Width / 2 + rndValX;
            var y = WindowPoint.Y + rect.Y + rect.Height / 2 + rndValY;

            WinApi.MoveOnPoint(IntPtr.Zero, new Point(x, y));

            return true;
        }

        bool ClickShort(Rectangle rect, int buttonId)
        {
            
            var rnd = new Random();
            var rndValX = rnd.Next(0, 2);
            var rndValY = rnd.Next(0, 2);

            var x = WindowPoint.X + rect.X + rect.Width/2  + rndValX;
            var y = WindowPoint.Y + rect.Y + rect.Height/2  + rndValY;

            //if (ClickLog != null)
            //{
            //    ClickLog(Algo + ":" + buttonId.ToString());
            //}

            WinApi.MoveOnPoint(IntPtr.Zero, new Point(x, y));
            WinApi.ClickOnPoint(IntPtr.Zero, new Point(x, y));


            return true;
        }

        public bool ClickCheck()
        {
            return Click(ButtonTip.Check, false);
        }
        Bitmap GetArea(Bitmap bmp, Rectangle rect)
        {
            //var newBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            //Graphics g = Graphics.FromImage(bmp);

            var crop = new Crop(rect);
            var newBmp = crop.Apply(bmp);
            return newBmp;
        }

        public string GetRound()
        {
            if (IsFlop) return "flop";
            if (IsTurn) return "turn";
            if (IsRiver) return "river";
            return "pre-flop";
        }

        public string GetMyCardsStr()
        {
            var str = "";
            foreach (Card card in MyCards)
            {
                str += card.ToShortStr() + ",";
            }
            if (str.Length > 0)
                str = str.Substring(0, str.Length - 1);

            return str;
        }

        public string GetAllCardsStr()
        {
            var str = "";
            foreach (Card card in AllCards)
            {
                str += card.ToShortStr() + ",";
            }

            if(str.Length > 0)
                str = str.Substring(0, str.Length - 1);
			else
				str = "none";

            return str;
        }



        public string GetPlayerStr()
        {
            var res = "";
            for (int i = 0; i < Players.Count; i++)
            {
                
                var player = Players[i];
                res += player.GetAction + "|";
                
            }
            return res;
        }

        public string GetPlayerStrLong()
        {
            var res = "";
            for (int i = 0; i < Players.Count; i++)
            {

                var player = Players[i];
                res += player.ToString() + "|";
                
            }
            return res;
        }

        public bool LimitByBet 
        {
            get
            {
                return Bank.CallValue <= MinBet * ((decimal)6) && Bank.CallValueValid && Bank.CallValue > 0;
            }
        
        }

        public bool LimitByPosa 
        {
            get
            {
                return Bank.CallValue <= MyBalance.Posa / ((decimal)6) && Bank.CallValueValid;
            }
        }

        public Rectangle BetInput { get; set; }

        public Bitmap ButtonFoldBmp { get; set; }
        public Bitmap ButtonCallCheckBmp { get; set; }
        public Bitmap ButtonBetRaiseBmp { get; set; }

        public int FoldPlayers
        {
            get { return Players.Count(player => !player.IsMe && player.IsFold && player.IsValid); }
        }

        public int ActivePlayers 
        { 
            get { return Players.Count(player => !player.IsMe && (player.IsActive || player.IsAllIn) && !player.IsFold); }
        }

        public int TotalPlayers
        {
            get { return Players.Count(player => (player.IsActive|| player.IsAllIn)); }
        }

        public bool IsAllIn
        {
            get
            {
                return
                    MyBalance.Posa <= Bank.CallValue && Bank.CallValueValid
                    ||
                    MyBalance.Posa <= Bank.RaiseValue && Bank.RaiseValueValid;
            }
        }
        //public int TotalPlayers
        //{
        //    get
        //    {
        //        var newPlayers = new List<Player>(Players);

        //        int i = 0;
        //        while (i < newPlayers.Count)
        //        {
        //            if (!newPlayers[i].IsActive)
        //            {
        //                newPlayers.RemoveAt(i);
        //            }
        //            else
        //                i++;
        //        }

        //        return newPlayers.Count;
        //    }
        //}

        public bool IsLittleBlind { get; set; }

        public Rectangle PauseRect { get; set; }
        public Rectangle ResumeRect { get; set; }

        public bool IsPaused
        {
            get
            {
                var img = GetAreaFromScreen(ResumeRect);
                return _cardRecognizer.ScanByTemplate(img, Resources._continue);
            }
        }

        public void ClickPause()
        {
            if (PauseRect != Rectangle.Empty)
            {
                var rnd = _random.Next(0, 3);
                var pt = new Point(
                    WindowPoint.X + PauseRect.X + PauseRect.Width / 2 + rnd,
                    WindowPoint.Y + PauseRect.Y + PauseRect.Height / 2);

                WinApi.ClickOnPoint(IntPtr.Zero, pt);
                
                var x = _random.Next(5, 10);
                var y = _random.Next(5, 10);
                WinApi.MoveOnPoint(IntPtr.Zero, new Point(pt.X + x, pt.Y + y));
            }
        }


        Random _playerRandom = new Random();
        public void MoveToRandomPlayer()
        {
            var val = _playerRandom.Next(0, 100);

            var idx = _playerRandom.Next(0, Players.Count - 1);
            if (val < 3)
            {
                if(Players.Count - 1 >= idx && Players[idx].Rect != Rectangle.Empty)
                {
                    var xRnd = _random.Next(0, 10);
                    var yRnd = _random.Next(0, 10);
                    var pt = new Point(WindowPoint.X + Players[idx].Rect.X + xRnd, WindowPoint.Y + Players[idx].Rect.Y + yRnd);
                    WinApi.MoveOnPoint(IntPtr.Zero, pt);
                }
            }
        }

        public void ClickOnPoint(Point pt)
        {
            WinApi.ClickOnPoint(IntPtr.Zero, pt);
        }

        public void TypeNumber(decimal p)
        {
            WinApi.TypeNumber(p);
        }

        public bool IsOpened
        {
            get
            {
                var title = WinApi.GetActiveWindowTitle();
                if (title.ToLower().Contains("холдем"))
                    return true;
                return false;
            }
        }

        public bool Open()
        {
            Thread.Sleep(300);

            var title = WinApi.GetActiveWindowTitle();
            if (title.ToLower().Contains("лобби"))
            {
                var pt = new Point(RectOpenTable.Location.X, RectOpenTable.Location.Y);
                WinApi.MoveOnPoint(IntPtr.Zero, pt);
                DoubleClick(RectOpenTable);
                return Open();
            }
            if (title.ToLower().Contains("найти место"))
            {
                var pt = new Point(RectCancelTable.Location.X, RectCancelTable.Location.Y);
                WinApi.MoveOnPoint(IntPtr.Zero, pt);
                WinApi.ClickOnPoint(IntPtr.Zero, pt);
                return Open();
            }

            if (title.ToLower().Contains("бай-ин"))
            {
                var pt = new Point(WindowPoint.X + RectBuyInOk.Location.X, WindowPoint.Y + RectBuyInOk.Location.Y);
                WinApi.MoveOnPoint(IntPtr.Zero, pt);
                WinApi.ClickOnPoint(IntPtr.Zero, pt);
                return Open();
            }

            if (title.ToLower().Contains("холдем"))
                return true;

            return false;
        }
        
        
        public Rectangle RectOpenTable { get; set; }
        public Rectangle RectCancelTable { get; set; }
        public Rectangle RectBuyInOk { get; set; }


        public Rectangle RectTableX { get; set; }
        public Rectangle RectLeaveTableOk { get; set; }
        
        public string TableName
        {
            get
            {
                return WinApi.GetActiveWindowTitle();
            }
        }
        public bool Close()
        {
            Thread.Sleep(300);

            var title = WinApi.GetActiveWindowTitle();
            if (title.ToLower().Contains("холдем"))
            {
                var pt = new Point(WindowPoint.X + RectTableX.Location.X, WindowPoint.Y + RectTableX.Location.Y);
                WinApi.MoveOnPoint(IntPtr.Zero, pt);
                WinApi.ClickOnPoint(IntPtr.Zero, pt);
                return Close();
            }

            if (title.ToLower().Contains("стол"))
            {
                var pt = new Point(WindowPoint.X + RectLeaveTableOk.Location.X, WindowPoint.Y + RectLeaveTableOk.Location.Y);
                WinApi.MoveOnPoint(IntPtr.Zero, pt);
                WinApi.ClickOnPoint(IntPtr.Zero, pt);
                return true;
            }

            return false;
        }

        public string TxtResult { get; set; }
    }
}