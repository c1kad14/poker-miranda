using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using LogLoader;
using log4net;
using miranda.JabberBot;
using miranda.Properties;
using miranda.ui;
using System.Drawing.Imaging;

namespace miranda
{
    public partial class TableForm : Form, IAction
    {
        KeyboardHook _hook = new KeyboardHook();
        private Bitmap _playersTemplate;
        readonly Random _random;
        public TableForm()
        {
            InitializeComponent();
            Text = "build on " +  new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd-MM-yyyy HH:mm:ss");
            SyncContext = SynchronizationContext.Current;

            _log = LogManager.GetLogger("Application");
            _logDebug = LogManager.GetLogger("Debug");
            
            _hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(_hook_KeyPressed);
            _hook.RegisterHotKey(miranda.ModifierKeys.Win, Keys.Escape);
            
            cbAlgo.SelectedItem = "1";

            _log.Info("app-start");

            Width = 500;
            _random = new Random();
        }

        

        void _hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            
            if (e.Modifier == miranda.ModifierKeys.Win)
            {
                timer1.Enabled = !timer1.Enabled;
                if (timer1.Enabled)
                {
                    StartTrade();
                }
                else
                {
                    StopTrade();
                }
            }
            
        }

        void LogApp(string msg, bool getPosa)
        {
            var str = "";
            if (_table != null && getPosa)
            {
                if (_table.MyBalance != null)
                {
                    str = _table.MyBalance.Posa.ToString();
                }
            }
            _log.Info("service;" + str + ";" + msg);
        }

        private bool _isInit = true;
        void StartTrade()
        {
            if (_isInit)
            {
                _pauseMinutes = _swRandom.Next(7, 15);
                _sw.Reset();
                _swPause.Reset();
            }
            _sw.Start();
            _swPause.Start();

            
            //_isPaused = false;

            Text = "Table: running";
            DisableControls();

            LogApp("trade_start", false);
        }

        void StopTrade()
        {
            _isInit = false;
            _swPause.Stop();
            _sw.Stop();
            Text = "Table: stopped";
            EnableControls();

            var str = "0";
            if (_table != null)
            {
                if (_table.MyBalance != null)
                {
                    str = _table.MyBalance.Posa.ToString();
                }
            }
            LogApp("trade_end", false);
        }

        readonly Dictionary<string, bool> _ctrlState = new Dictionary<string, bool>();

        private void EnableControls()
        {
            foreach (Control control in this.Controls)
            {
                if (_ctrlState.ContainsKey(control.Name))
                    control.Enabled = _ctrlState[control.Name];
                else
                    control.Enabled = true;
            }
        }

        private void DisableControls()
        {
            foreach (Control control in this.Controls)
            {
                if (control.Name == cbShowLog.Name || control.Name == tbLog.Name)
                    continue;

                if (_ctrlState.ContainsKey(control.Name))
                {
                    _ctrlState[control.Name] = control.Enabled;
                    control.Enabled = false;
                }
                else
                {
                    _ctrlState.Add(control.Name, control.Enabled);
                    control.Enabled = false;
                }
            }
        }


        Stack<FileInfo> _files = new Stack<FileInfo>();
        Stack<FileInfo> _filesBack = new Stack<FileInfo>();

        private void btnNextFile_Click(object sender, EventArgs e)
        {
            
            var di = new DirectoryInfo(tbDirTestFiles.Text);

            if (_files.Count == 0)
            {
                var files = new List<FileInfo>( di.GetFiles("*.jpeg"));
                files.AddRange(di.GetFiles("*.bmp"));
                var orderedFiles = files.OrderByDescending(f => f.LastWriteTime);
                //files.Sort((info, fileInfo) => fileInfo.Name.CompareTo(info.Name));

                foreach (var fileInfo in orderedFiles)
                {
                    _files.Push(fileInfo);
                }
            }

            var fi = _files.Pop();
            
            lblCurrentFile.Text = fi.Name;
            var image = Bitmap.FromFile(fi.FullName) as Bitmap;
            if (cbDebug.Checked)
                pbDebug.Image = image;
            else
            {
                pbDebug.Image = null;
            }

            RecognizeTable(image);
            //Algo1(true);
            
        }

        private void btnRenew_Click(object sender, EventArgs e)
        {
            _files.Clear();
        }

        private Table _table;
        private ILog _log;
        private ILog _logDebug;


        void RecognizeTable(Bitmap image)
        {
            if (_table == null)
            {
                _table = new Table((rbSix.Checked ? 6 : 9), _settings);
                _table.ClickLog += TableOnClickLog;
                _table.ClickLogDebug += TableOnClickLogDebug;
                _table.AfterClickLog +=TableOnAfterClickLog;
                InitPlayerRects(_settings);
                InitButtonRects(_settings);
                InitBetRects(_settings);
            }
            else
            {
                _table.ClearTable();
            }

            var dir = Application.StartupPath + @"\Temp\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _table.TempDir = dir;
            _table.MinBet = _settings.MinBet;

            _table.BetInput = _settings.BetInputRect;

            _table.WindowPoint = _settings.WindowPoint;
            

            _table.RecognizeFromWholeBmp(image, _settings);

            tbTxtResult.Text = _table.TxtResult;
            
            var diff = _settings.MinBet;
            foreach (var player in _table.Players)
            {
                player.IsAllIn = false;
                player.Parse(_settings.RemoveDollar);
                if (player.IsActive)
                {
                    var newDiff = player.AnalyseBets(diff);
                    if (player.Action == PlayerAction.Raise)
                    {
                        diff = newDiff;
                    }
                    //Ex.Info(player.Posa + ";" + player.PosaPrev);
                }
                if (!player.IsFold && !player.IsActive)
                {
                    player.IsAllIn = true;
                }

                player.Stat.Collect(player);
                //if(player.IsActive)
                //    player.AnalyseBets(_settings.MinBet);
            }
            ////TODO players
            //var crop = new Crop(_settings.Player1);
            //var playerBmp = crop.Apply(image);
            //_table.RecognisePlayer(playerBmp);

            //TODO SLOWWW
            //_table.CountPlayers(_playersTemplate);

            _table.SetRaiserPosition();

            lblPlayersCount.Text = 
                _table.TotalPlayers + " players, " 
                + _table.FoldPlayers + " fold, " 
                + _table.RaiserCount + " raised at " + _table.RaiserPosition + ","
                + "button=" + (_table.ButtonPlayer + 1);

            lblDealerButton.Text = (_table.DealerButtons.IndexOf(true) + 1).ToString() + " - " + _table.MyPosition;

            pbAllCards.Image = _table.AllCardsBmp;
            pbMyCards.Image = _table.MyCardsBmp;

            
            pbButtonsFold.Image = _table.ButtonFoldBmp;
            pbButtonsCallCheck.Image = _table.ButtonCallCheckBmp;
            pbButtonsBetRaise.Image = _table.ButtonBetRaiseBmp;
            

            rbAllCards.Text = "";
            rbMyCards.Text = "";
            foreach (var item in _table.AllCards.CardsStr)
            {
                rbAllCards.AppendText(item.Item1, item.Item2);
            }
            foreach (var item in _table.MyCards.CardsStr)
            {
                rbMyCards.AppendText(item.Item1, item.Item2);
            }
            

            tbButtons.Text = _table.Buttons.ToString();

            pbMyBalance.Image = _table.MyBalance.Image;
            tbMyBalance.Text = _table.MyBalance.ToString();

            pbBank.Image = _table.Bank.BankImg;
            tbBank.Text = _table.Bank.BankValue.ToString();

            pbCall.Image = _table.Bank.CallImg;
            tbCall.Text = _table.Bank.CallValue.ToString();

            pbRaise.Image = _table.Bank.RaiseImg;
            tbRaise.Text = _table.Bank.RaiseValue.ToString();

            SetPlayerUi(tb1, pb1, 0);
            SetPlayerUi(tb2, pb2, 1);
            SetPlayerUi(tb3, pb3, 2);
            SetPlayerUi(tb4, pb4, 3);
            SetPlayerUi(tb5, pb5, 4);
            SetPlayerUi(tb6, pb6, 5);
            SetPlayerUi(tb7, pb7, 6);
            SetPlayerUi(tb8, pb8, 7);
            SetPlayerUi(tb9, pb9, 8);

            
            SetBetUi(tbBet1, pbBet1, 0);
            SetBetUi(tbBet2, pbBet2, 1);
            SetBetUi(tbBet3, pbBet3, 2);
            SetBetUi(tbBet4, pbBet4, 3);
            SetBetUi(tbBet5, pbBet5, 4);
            SetBetUi(tbBet6, pbBet6, 5);
            SetBetUi(tbBet7, pbBet7, 6);
            SetBetUi(tbBet8, pbBet8, 7);
            SetBetUi(tbBet9, pbBet9, 8);
            
        }

        private bool _collectAfterInfo;
        private string _afterClickInfo = "";
        private void TableOnAfterClickLog(string msg)
        {
            _collectAfterInfo = true;
            _afterClickInfo = msg;
        }

        void SetPlayerUi(TextBox txt, PictureBox pic, int id)
        {
            if (_table.Players.Count > id)
            {
                txt.Text = _table.Players[id].ToString();
                pic.Image = _table.Players[id].Image;
            }
        }

        void SetBetUi(TextBox txt, PictureBox pic, int id)
        {
            if (_table.PlayerBets.Count > id)
            {
                txt.Text = _table.PlayerBets[id].ToString();
                pic.Image = _table.PlayerBets[id].Image;
            }
        }

        private void TableOnClickLog(string msg)
        {
            //var state = ";;";
            //if (_table != null)
            //{
            //    if(_table.IsPreFlop)
            //        state = _table.BetCounterPreFlop + ";" + _table.GetStateStr();

            //    if (_table.IsFlop)
            //        state = _table.BetCounterFlop + ";" + _table.GetStateStr();

            //    if (_table.IsTurn)
            //        state = _table.BetCounterTurn + ";" + _table.GetStateStr();

            //    if (_table.IsRiver)
            //        state = _table.BetCounterRiver + ";" + _table.GetStateStr();
            //}

            var state1 = (_table != null)
                ?
                    _table.GetRound() + ";"
                    + CurrentCounter + ";"
                    + _table.GetAllCardsStr() + ";"
                    + _table.GetMyCardsStr() + ";"
                    + _table.MyPosition + ";"
                    + _table.RaiserCount + " raised at " + _table.RaiserPosition + ";"
                    + "call=" + _table.Bank.CallValue + ";"
                    + "raise=" + _table.Bank.RaiseValue + ";"
                    + "bank=" + _table.Bank.BankValue + ";"
                    + _table.TotalPlayers + ";"
                    + _table.Bank.CallStr.Trim() + "," + _table.Bank.RaiseStr.Trim() + "," + _table.Bank.BankStr.Trim() + ";" 
                    
                :
                    ";;;;;";
            var state2 = (_table != null) 
                ? 
                     "IsAllIn=" + _table.IsAllIn + ";"
                    + "NoRaise=" + _table.NoRaise + ";"
                    + "FreePlay=" + _table.IsFreePlay + ";"
                    + "Initiative="+ _table.IsInitiative + ";"
                    + _table.GetPlayerStr() + ";"
                    + _table.GetPlayerStrLong() + ";"
                :
                    ";;";

            var infoStr = msg + ";" + _table.MyBalance.Posa + ";" + _table.Bank.TotalValue + ";" + state1 + state2;
            _log.Info(infoStr);
            lblStatus.Text = infoStr;

            var str1 = msg + ";" + _table.MyBalance.Posa + ";" + _table.Bank.TotalValue + ";" + state1;
            var str2 = (_table != null)
                           ? "IsAllIn=" + _table.IsAllIn + ";"
                             + "NoRaise=" + _table.NoRaise + ";"
                             + "FreePlay=" + _table.IsFreePlay + ";"
                             + "Initiative="+ _table.IsInitiative + ";      "
                             + _table.GetPlayerStr()
                           : "";
            var str3 = (_table != null) ? _table.GetPlayerStrLong() : "";
            SaveBmpLog(str1, str2, str3);
            //if (cbSaveTableBmp.Checked)
            //{
            //    var bmp = _table.GetAreaFromScreen(_settings.TableRect);

            //    var dir = Application.StartupPath + @"\pics\";
            //    if (!Directory.Exists(dir))
            //        Directory.CreateDirectory(dir);
            //    int i = 0;
            //    while (File.Exists(dir + i + ".bmp"))
            //        i++;

            //    var graphicImage = Graphics.FromImage(bmp);

            //    graphicImage.FillRectangle(new SolidBrush(Color.Black), new Rectangle(new Point(10, 30), new Size(bmp.Width, 70)));
            //    graphicImage.DrawString(msg + ";" + _table.MyBalance.Posa + ";" + state1, new Font("Arial", 10, FontStyle.Bold),
            //        new SolidBrush(Color.White),
            //       //SystemBrushes.WindowText, 
            //       new Point(10, 30));
            //    graphicImage.DrawString((_table != null) ? 
            //         "IsAllIn=" + _table.IsAllIn + ";"
            //        + "NoRaise=" + _table.NoRaise + ";"
            //        + "FreePlay=" + _table.IsFreePlay + ";      "
            //        + _table.GetPlayerStr() :"", new Font("Arial", 10, FontStyle.Bold),
            //        new SolidBrush(Color.White),
            //        //SystemBrushes.WindowText, 
            //       new Point(10, 50));
            //    graphicImage.DrawString((_table != null) ? _table.GetPlayerStrLong() : "", new Font("Arial", 8, FontStyle.Bold),
            //        new SolidBrush(Color.White),
            //        //SystemBrushes.WindowText, 
            //       new Point(10, 70)); 
            //    bmp.Save(dir + i + ".bmp");
            //}
        }


        int _fileCounter = 0;
        void SaveBmpLog(string str1, string str2, string str3)
        {
            if (cbSaveTableBmp.Checked)
            {
                var bmp = _table.GetAreaFromScreen(_settings.TableRect);

                var dir = Application.StartupPath + @"\pics\";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (_fileCounter == 0)
                {
                    while (File.Exists(dir + _fileCounter + ".jpeg"))
                        _fileCounter++;
                }
                _fileCounter++;

                var bmpExt = new Bitmap(bmp.Width, bmp.Height + 100);

                var graphicImage = Graphics.FromImage(bmpExt);
                //graphicImage.FillRectangle(new SolidBrush(Color.White), new Rectangle(new Point(0, 0), bmpExt.Size));

                graphicImage.DrawImage(bmp, new Rectangle(new Point(0, 0), bmp.Size), new Rectangle(new Point(0, 0), bmp.Size), GraphicsUnit.Pixel);

                int add = bmp.Height;

                if (!string.IsNullOrEmpty(str1) || !string.IsNullOrEmpty(str2) || !string.IsNullOrEmpty(str3))
                {
                    graphicImage.FillRectangle(new SolidBrush(Color.Black),
                                               new Rectangle(new Point(10, 30 + add), new Size(bmp.Width, 70)));
                }

                if (!string.IsNullOrEmpty(str1))
                {
                    graphicImage.DrawString(str1,
                                            new Font("Arial", 10, FontStyle.Bold),
                                            new SolidBrush(Color.White),
                                            new Point(10, 30 + add));
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    graphicImage.DrawString(str2,
                                            new Font("Arial", 8, FontStyle.Bold),
                                            new SolidBrush(Color.White),
                                            new Point(10, 50 + add));
                }

                if (!string.IsNullOrEmpty(str3))
                {
                    graphicImage.DrawString(str3,
                                            new Font("Arial", 8, FontStyle.Bold),
                                            new SolidBrush(Color.White),
                                            new Point(10, 70 + add));
                }


                var myEncoderParameters = new EncoderParameters(1);

                var myEncoderParameter = new EncoderParameter(Encoder.Quality, 70L);
                var fi = new FileInfo(openFileDialog1.FileName);
                myEncoderParameters.Param[0] = myEncoderParameter;
                var jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                bmpExt.Save(dir + _fileCounter + ".jpeg", jgpEncoder, myEncoderParameters);

                //bmp.Save(dir + i + ".bmp");
            }
        }

        

        private void btnSelectTestDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDirTestFiles.Text = folderBrowserDialog1.SelectedPath;
                _settings.TestFolder = tbDirTestFiles.Text;
                AppSettingsManager.Save(_settings);
            }
        }


        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        private void btnTest_Click(object sender, EventArgs e)
        {
            var ptr = WndSearcher.SearchForWindow("", "WinRAR");
            WndSearcher.SetForegroundWindow(ptr);
            return;
            //_table = new Table(0, _settings);
            _table.WindowPoint = _settings.WindowPoint;
            SaveBmpLog("test", "test2", "test3");
            return;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var image = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
                var myEncoderParameters = new EncoderParameters(1);

                var myEncoderParameter = new EncoderParameter(Encoder.Quality, 70L);
                var fi = new FileInfo(openFileDialog1.FileName);
                myEncoderParameters.Param[0] = myEncoderParameter;
                var jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                image.Save(fi.Directory.FullName + @"\1.jpeg", jgpEncoder, myEncoderParameters);
            }
            return;

            _table = new Table(6, AppSettingsManager.Load("six"));
            var pt = tbLog.PointToScreen(tbLog.Location);
            _table.ClickOnPoint(pt);
            _table.TypeNumber((decimal)0.98);
            return;
            var txt = "test	t   t";
            var t = txt.Replace("\t", string.Empty);
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
            //lblNextPause.Text = _table.IsPaused.ToString();
            return;
            //_logDebug.Info("test");
            //return;
            //_table.BetInput = _settings.BetInputRect;
            //_table.AllCards = new CardCollection();
            //_table.MyCards = new CardCollection();
            //_table.ClickBet((decimal)0.05);
            //return;
            //_table.ClickButton2();
            //return;

            //var str = "1";
            //str = str.Substring(1);
            //var pl = new PlayerCollection();
            //pl.Add(new Player {Action = PlayerAction.Check });
            //pl.Add(new Player { Action = PlayerAction.Check });
            //pl.Add(new Player { Action = PlayerAction.Check });
            //pl.Add(new Player { Action = PlayerAction.Check });
            //pl.Add(new Player { Action = PlayerAction.Call });
            //pl.Add(new Player { Action = PlayerAction.Check, IsMe = true });

            //_table.Players = pl;

            //Trace.WriteLine(_table.DdAllCalled + ","  +_table.DdAllCheckOrFold
            //    + "," + _table.DdOneRaisedAllFold
            //    +"," + _table.DdOneRaisedOneCalled
            //    );
            //return;

            /*
            _table.MyCards = new CardCollection();
            _table.MyCards.Clear();
            _table.MyCards.Add(new Card {Rank = Rank.Queen, Suit = Suit.Diamonds});
            _table.MyCards.Add(new Card { Rank = Rank.King, Suit = Suit.Spades });
            Trace.WriteLine(_table.MyCards.HasCards("T.,KQs"));
            Trace.WriteLine(_table.MyCards.HasCards("T.o,KTo"));
            Trace.WriteLine(_table.MyCards.HasCards("T.s,KQo"));
            return;
            var r = CheckPair(
                new Card {Rank = Rank.Ten},
                new Card {Rank = Rank.Four},
                new Card {Rank = Rank.Four},
                null, //new Card {Rank = Rank.King},
                null, //new Card {Rank = Rank.Four},
                
                new Card {Rank = Rank.Two},
                new Card {Rank = Rank.Ten},

                Rank.Four
                );
            Trace.WriteLine(r);
            return;
            //Ex.Report(new Exception("test"));
            //*/

            var ids = new Dictionary<Rank, int>()
                {
                    {Rank.Two,  0},
                    {Rank.Three,  0},
                    {Rank.Four,  0},
                    {Rank.Five,  0},
                    {Rank.Six,  0},
                    {Rank.Seven,  1},
                    {Rank.Eight,  0},
                    {Rank.Nine,  0},
                    {Rank.Ten,  1},
                    {Rank.Jack,  1},
                    {Rank.Queen,  1},
                    {Rank.King,  1},
                    {Rank.Ace,  1},
                };
            var cards = new List<Card>();

            foreach (KeyValuePair<Rank, int> pair in ids)
            {
                if (pair.Value == 1)
                {
                    cards.Add(new Card(){Rank = pair.Key});
                }
            }

            Trace.WriteLine("street = " + CheckStreetAlgo(cards.ToArray(), 5));
            Trace.WriteLine("half-street = " + CheckHalfStreetAlgo(cards.ToArray(), 4));
            lblStatus.Text =((int)(Rank.Ace & Rank.King)).ToString();
            //if (_table != null)
            //{
            //    _table.ClickFold();
            //}
        }

        private int _cnt = 0;
        private int _cntValue = 1;

        readonly Stopwatch _swPause = new Stopwatch();
        int _pauseMinutes;
        bool _isPaused;

        readonly Stopwatch _sw = new Stopwatch();
        readonly Random _swRandom = new Random();

        readonly Random _algoRandom = new Random();
        private string _lastTableName = "";
        private string _currentTableName = "";
        private string _lastCloseReason = "";
        private string _lastRebuy = "";
        private int _rebuyCount;

        private int _roundsPlayed;
        decimal _lastTableTotal = 0;
        int _bluff = 100;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            //bool doStart = true;
            try
            {
                
                //if (cbDebug.Checked)
                //{
                //    //if (cbSaveTableBmp.Checked)
                //    //{
                //    //    var bmp = _table.GetAreaFromScreen(_settings.TableRect);

                //    //    var dir = Application.StartupPath + @"\pics\";
                //    //    if (!Directory.Exists(dir))
                //    //        Directory.CreateDirectory(dir);
                //    //    int i = 0;
                //    //    while (File.Exists(dir + i + ".bmp"))
                //    //        i++;

                //    //    var graphicImage = Graphics.FromImage(bmp);
                //    //    bmp.Save(dir + i + ".bmp");
                //    //}
                //}

                

                int algo;
                int.TryParse(cbAlgo.SelectedItem.ToString(), out algo);


                if (_table == null)
                {
                    _table = new Table((rbSix.Checked ? 6 : 9), _settings);
                    _table.ClickLog += TableOnClickLog;
                    _table.ClickLogDebug += TableOnClickLogDebug;
                    _table.AfterClickLog += TableOnAfterClickLog;
                    InitPlayerRects(_settings);
                    InitButtonRects(_settings);
                    InitBetRects(_settings);
                }
                else
                {
                    _table.ClearTable();
                }

                
                _table.WindowPoint = _settings.WindowPoint;
                

                _table.RecognizeFromWin(_settings);

                if (_collectAfterInfo)
                {
                    CollectAfterInfo();
                    _collectAfterInfo = false;
                    _afterClickInfo = "";
                }

                #region TablePause
                
                

                if (cbUsePause.Checked && _swPause.Elapsed >= TimeSpan.FromMinutes(_pauseMinutes))
                {
                    _table.ClickPause();

                    _isPaused = !_isPaused;
                    if (_isPaused)
                        _pauseMinutes = _swRandom.Next(1, 2);
                    else
                        _pauseMinutes = _swRandom.Next(90, 120);

                    _swPause.Reset();
                    _swPause.Restart();
                }

                bool tableIsPaused = _table.IsPaused;

                if (
                    !(
                        tableIsPaused && _isPaused
                        ||
                        (!tableIsPaused && !_isPaused)
                    )
                   )
                {
                    _table.ClickPause();
                }

                lblNextPause.Text = tableIsPaused + ", pause in " + (TimeSpan.FromMinutes(_pauseMinutes) - _swPause.Elapsed).ToString(@"hh\:mm\:ss");
                
                #endregion

                //****************************************************
#if !DEBUG
                if (!_table.IsOpened)
                {
                    if (TryActivate())
                        return;

                    _table.Open();
                    _currentTableName = _table.TableName;

                    _table.ClearTable();
                    _table.WindowPoint = _settings.WindowPoint;
                    _table.RecognizeFromWin(_settings);
                    _lastTableTotal = _table.Bank.TotalValue;

                    SaveBmpLog("", "", "");

                    LogApp("table opened: " + _currentTableName + " at " + DateTime.Now.ToString("HH:mm:ss"), false);
                    return;
                }

                if ((_table.MyBalance.Posa >= _settings.Limit || (_table.TotalPlayers < 6)) && _table.IsValid && _table.IsPreFlop)
                {
                    _lastCloseReason = (_table.MyBalance.Posa >= _settings.Limit ? "win" : "no players") + " at " + DateTime.Now.ToString("HH:mm:ss");
                    _lastTableName = _table.TableName;

                    SaveBmpLog("", "", "");

                    if (!_table.IsPaused)
                        _table.ClickPause();

                    _table.Close();

                    LogApp("table closed: " + _lastCloseReason, true);
                    return;
                }

                int stackCnt = 2;
                if (_table.IsValid && _table.IsPreFlop
                    && _table.Bank.TotalValueValid && _lastTableTotal != 0
                    && _lastTableTotal - _table.Bank.TotalValue >= _settings.BuyIn * stackCnt
                    )
                {
                    _lastTableTotal = _table.Bank.TotalValue;

                    _lastCloseReason = stackCnt + " stacks down at " + DateTime.Now.ToString("HH:mm:ss");
                    _lastTableName = _table.TableName;

                    SaveBmpLog("", "", "");

                    if (!_table.IsPaused)
                        _table.ClickPause();

                    _table.Close();

                    LogApp("table closed: " + _lastCloseReason, true);
                    return;
                }
#endif
                //****************************************************

                _table.MoveToRandomPlayer();

                _table.BetInput = _settings.BetInputRect;
                _table.MinBet = _settings.MinBet;
                //TODO в безлимитном $1/$2 - малый/большой блайнды, в лимитном - ставки префлоп-флоп/терн-ривер
                //if(_table.IsPreFlop || _table.IsFlop)
                //    _table.MinBet = _settings.MinBet;
                //else
                //    _table.MinBet = _settings.MinBet * 2;

                //TODO SLOWWW
                //if(algo > 3)
                //    _table.CountPlayers(_playersTemplate);


                if (cbSetUi.Checked)
                {
                    pbAllCards.Image = _table.AllCardsBmp;
                    pbMyCards.Image = _table.MyCardsBmp;


                    pbButtonsFold.Image = _table.ButtonFoldBmp;
                    pbButtonsCallCheck.Image = _table.ButtonCallCheckBmp;
                    pbButtonsBetRaise.Image = _table.ButtonBetRaiseBmp;

                    pbMyBalance.Image = _table.MyBalance.Image;
                    pbBank.Image = _table.Bank.BankImg;
                    pbCall.Image = _table.Bank.CallImg;
                    pbRaise.Image = _table.Bank.RaiseImg;

                    pbTotal.Image = _table.Bank.TotalImg;
                }
                
                rbAllCards.Text = "";
                rbMyCards.Text = "";
                foreach (var item in _table.AllCards.CardsStr)
                {
                    rbAllCards.AppendText(item.Item1, item.Item2);
                }
                foreach (var item in _table.MyCards.CardsStr)
                {
                    rbMyCards.AppendText(item.Item1, item.Item2);
                }

                tbButtons.Text = _table.Buttons.ToString();


                lbBalance.Text = "last=" + _lastTableTotal + " ,current=" + _table.Bank.TotalValue + " ,bluff=" + _bluff;
                
                tbMyBalance.Text = _table.MyBalance.ToString();

                
                tbBank.Text = _table.Bank.BankValue.ToString();

                
                tbCall.Text = _table.Bank.CallValue.ToString();

                
                tbRaise.Text = _table.Bank.RaiseValue.ToString();
                tbTotal.Text = _table.Bank.TotalValue.ToString();

                if (_table.MyCards.Count == 0)
                {
                   
                    _table.BetCounterPreFlop = 1;
                    _table.BetCounterFlop = 1;
                    _table.BetCounterTurn = 1;
                    _table.BetCounterRiver = 1;
                    _table.RaiseCounter = 0;
                }

                lblBetCounter.Text = CurrentCounter + " bet counter";

                int rand = 100;
                
                if (_table.ButtonPlayer != _table.ButtonPlayerPrev)
                //начало раздачи
                {
                    _bluff = _bluffRandom.Next(0, 100);

                    _table.PlayerActionCounter = 0;
   
                    _table.TotalRounds += 1;
                    //_logDebug.Info("table begin");
                    //_logDebug.Info("ButtonPlayer=" + _table.ButtonPlayer + ", ButtonPlayerPrev=" + _table.ButtonPlayerPrev);

                    _table.IsInitiative = false;

                    var stat = "";
                    foreach (var player in _table.Players)
                    {
                        stat += player.Stat.GetStat() + ";";
                        player.ParsePrev(_settings.RemoveDollar);

                        if (!player.IsActive && player.IsFold)
                        {
                            player.Stat.Clear();
                        }
                    }
                    _table.ButtonPlayerPrev = _table.ButtonPlayer;

                    //_logDebug.Info(stat);

                    rand = _algoRandom.Next(0, 100);
                    //_logDebug.Info("ButtonPlayer=" + _table.ButtonPlayer + ", ButtonPlayerPrev=" + _table.ButtonPlayerPrev);
                }


                if (_table.IsBlind && _table.IsPreFlop && CurrentCounter == 1)
                {
                    _table.IsFreePlay = true;
                }
                else
                {
                    _table.IsFreePlay = false;
                }

                //if (_table.IsValidButtons)
                //{
                //    _table.ButtonPlayerPrev = _table.ButtonPlayer;
                //}

                var swr = _swRandom.Next(1, 2);

                lblTimerInterval.Text = _sw.Elapsed.ToString(@"hh\:mm\:ss\.fff");
                
                if (_sw.Elapsed.Seconds < swr)
                {
                    return;
                }
                _sw.Restart();

                if (_table.IsValid && (_table.Bank.RaiseValueValid || _table.Bank.CallValueValid))
                {
                    //======================
                    if (_table.IsPreFlop
                        && _table.PlayerActionCounter == 0
                        && _table.MyBalance.Posa != 0
                        && _table.MyBalance.PosaPrev / _table.MyBalance.Posa * 100 <= _settings.RebuyLevel
                        //&& _table.MyBalance.Posa - (_table.MyBalance.PosaPrev + _table.Bank.BankValue) > _settings.MinBet //но не выйграли
                        && _settings.BuyIn - _table.MyBalance.Posa <= _settings.MinBet //и стек около бай-ина
                        && _settings.BuyIn >= _table.MyBalance.Posa
                        //&& _table.MyBalance.IsValid
                        )
                    {
                        _lastRebuy = "" + _table.MyBalance.Posa + " at " + DateTime.Now.ToString("HH:mm:ss");
                        _rebuyCount++;
                        LogApp("possible rebuy " + _lastRebuy, true);
                    }

                    _table.MyBalance.PosaPrev = _table.MyBalance.Posa;

                    //======================
                    var diff = _settings.MinBet;
                    foreach (var player in _table.Players)
                    {
                        player.IsAllIn = false;
                        player.Parse(_settings.RemoveDollar);
                        if (player.IsActive)
                        {
                            var newDiff = player.AnalyseBets(diff);
                            if (player.Action == PlayerAction.Raise)
                            {
                                diff = newDiff;
                            }
                            //Ex.Info(player.Posa + ";" + player.PosaPrev);
                        }
                        if (!player.IsFold && !player.IsActive)
                        {
                            player.IsAllIn = true;
                        }

                        player.Stat.Collect(player);
                    }

                    //После анализа записываем в PosaPrev
                    foreach (var player in _table.Players)
                    {
                        player.PosaPrevLog = player.PosaPrev;
                        player.PosaPrev = player.Posa;
                        //player.ParsePrev(_settings.RemoveDollar);
                    }
                    
                    _table.SetRaiserPosition();

                    //if(algo == 1)
                    //    Algo1(true, rand);

                    //if (algo == 2)
                    //Algo2(true, rand);

                    if (_bluff < _settings.ShortStackPercent)
                    {
                        Algo1(true, rand, _bluff);
                    }
                    else
                    {
                        Algo2(true, rand, _bluff);
                    }



                    //if (algo == 1 || algo == 2)
                    //{
                    //    Algo1(true);
                    //}
                    ////useless
                    //if (algo == 2)
                    //{
                    //    if (!Algo1(false))
                    //        Algo2(true);
                    //}
                    //if (algo == 3)
                    //{
                    //    if (!Algo1(false))
                    //        if (!Algo2(false))
                    //            Algo3(true);
                    //}

                    //if (algo == 4)
                    //{
                    //    if (!Algo1(false))
                    //        if (!Algo2(false))
                    //            if (!Algo3(false))
                    //                Algo4(true);
                    //}

                    //if (algo == 5)
                    //{
                    //    if (!Algo1(false))
                    //        if (!Algo2(false))
                    //            if (!Algo3(false))
                    //                if(!Algo4(false))
                    //                    Algo1(true);
                    //}

                }

                if(cbSetUi.Checked)
                    SetPlayerUis();

                lblPlayersCount.Text =
                    _table.TotalPlayers + " players, "
                    + _table.FoldPlayers + " fold, "
                    + _table.RaiserCount + " raised at " + _table.RaiserPosition + ","
                    + "button=" + (_table.ButtonPlayer + 1);
                //lblPlayersCount.Text = _table.TotalPlayers + " players, " + _table.FoldPlayers + " fold, button=" + (_table.ButtonPlayer + 1);

                lblDealerButton.Text = (_table.DealerButtons.IndexOf(true) + 1).ToString() + " - " + _table.MyPosition;
            }
            catch (Exception ex)
            {
                Ex.Report(ex);
            }
            finally
            {
                //GC.Collect();
                timer1.Enabled = true;
                //if (doStart)
                //    timer1.Enabled = true;
                //else
                //{
                //    _log.Info("stopped by profit");
                //    StopTrade();
                //}
            }
        }

        private bool TryActivate()
        {
            bool res = false;

            var cash = WndSearcher.SearchForWindow("", "Касса");
            
            var c1 = WndSearcher.SetForegroundWindow(cash);

            Thread.Sleep(100);

            var lobby = WndSearcher.SearchForWindow("", "Лобби");
            var c2 = WndSearcher.SetForegroundWindow(lobby);

            Thread.Sleep(100);

            var table = WndSearcher.SearchForWindow("", "лимита Холдем");
            var c3 = WndSearcher.SetForegroundWindow(table);


            return c1 != 0 && c2 != 0 && c3 != 0;
        }

        private bool _isOneStrategy = true;

        private void CollectAfterInfo()
        {
            var state1 = (_table != null)
                ?
                    _table.GetRound() + ";"
                    + CurrentCounter + ";"
                    + _table.GetAllCardsStr() + ";"
                    + _table.GetMyCardsStr() + ";"
                    + _table.MyPosition + ";"
                    + _table.RaiserCount + " raised at " + _table.RaiserPosition + ";"
                    + "call=" + _table.Bank.CallValue + ";"
                    + "raise=" + _table.Bank.RaiseValue + ";"
                    + "bank=" + _table.Bank.BankValue + ";"
                    + _table.TotalPlayers + ";"
                    + _table.Bank.CallStr.Trim() + "," + _table.Bank.RaiseStr.Trim() + "," + _table.Bank.BankStr.Trim() + ";" 

                :
                    ";;;;;";
            var state2 = (_table != null)
                ?
                     "IsAllIn=" + _table.IsAllIn + ";"
                    + "NoRaise=" + _table.NoRaise + ";"
                    + "FreePlay=" + _table.IsFreePlay + ";"
                    + "Initiative=" + _table.IsInitiative + ";"
                    + _table.GetPlayerStr() + ";"
                    + _table.GetPlayerStrLong() + ";"
                :
                    ";;";

            var infoStr = _afterClickInfo + "[after-click];" + _table.MyBalance.Posa + ";" + _table.Bank.TotalValue + ";" + state1 + state2;
            _log.Info(infoStr);

            var str1 = _afterClickInfo + "[after-click];" + _table.MyBalance.Posa + ";" + _table.Bank.TotalValue + ";" + state1;
            var str2 = (_table != null)
                           ? "IsAllIn=" + _table.IsAllIn + ";"
                             + "NoRaise=" + _table.NoRaise + ";"
                             + "FreePlay=" + _table.IsFreePlay + ";"
                             + "Initiative=" + _table.IsInitiative + ";      "
                             + _table.GetPlayerStr()
                           : "";
            var str3 = (_table != null) ? _table.GetPlayerStrLong() : "";
            SaveBmpLog(str1, str2, str3);
        }

        private void SetPlayerUis()
        {
            SetPlayerUi(tb1, pb1, 0);
            SetPlayerUi(tb2, pb2, 1);
            SetPlayerUi(tb3, pb3, 2);
            SetPlayerUi(tb4, pb4, 3);
            SetPlayerUi(tb5, pb5, 4);
            SetPlayerUi(tb6, pb6, 5);
            SetPlayerUi(tb7, pb7, 6);
            SetPlayerUi(tb8, pb8, 7);
            SetPlayerUi(tb9, pb9, 8);

            SetBetUi(tbBet1, pbBet1, 0);
            SetBetUi(tbBet2, pbBet2, 1);
            SetBetUi(tbBet3, pbBet3, 2);
            SetBetUi(tbBet4, pbBet4, 3);
            SetBetUi(tbBet5, pbBet5, 4);
            SetBetUi(tbBet6, pbBet6, 5);
            SetBetUi(tbBet7, pbBet7, 6);
            SetBetUi(tbBet8, pbBet8, 7);
            SetBetUi(tbBet9, pbBet9, 8);
        }

        decimal GetRoundRaiseLimit
        {
            get
            {
                var rnd = new Random();
                var rndVal = rnd.Next(0, 100);

                var val = _settings.WaitRoundLimit + (rndVal > 50 ? 0 : -1);
                if (val <= 0)
                    val = 1;
                return val;
            }
        }

        decimal GetRoundCallLimit
        {
            get
            {
                var rnd = new Random();
                var rndVal = rnd.Next(0, 100);

                var val = _settings.PreFlopWaitRoundLimit + (rndVal > 50 ? 0 : -1);
                if (val <= 0)
                    val = 1;
                return val;
            }
        }

        #region Unused algo
        /*
        /// <summary>
        /// Если домашние пары
        /// </summary>
        /// <param name="fold"></param>
        /// <returns></returns>
        private bool Algo1(bool fold)
        {
            _table.Algo = "algo1[hand-pair]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (card1.Rank == card2.Rank &&  card1.Rank >= Rank.Ten)
            {
				
                if (_table.AllCards.Count == 0)
                {
                    if (CurrentCounter <= GetRoundRaiseLimit)
                        return BetOrRaise();
                    else
                        return CheckOrCall();
                }

                if (_table.AllCards.Count > 0)
                {
                    if (_table.AllCards[_table.AllCards.Count - 1].Rank <= card1.Rank)
                    {
                        if (CurrentCounter <= GetRoundRaiseLimit)
                            return BetOrRaise();
                        else
                            return CheckOrCall();
                        
                    }
                    else
                    {
                         return CheckOrCall();
                    }
                    if(fold)
                        if (_table.ClickCheck())
                            return true;

                    if (fold)
                        _table.ClickFold();
                    return false;
                }
				
                return CheckOrCall();

            }
            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        
        /// <summary>
        /// Связки
        /// </summary>
        /// <param name="fold"></param>
        /// <returns></returns>
        private bool Algo2(bool fold)
        {

            _table.Algo = "algo2[street]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (card2.Rank - card1.Rank == 1 && 
                (
                card1.Rank >= Rank.Ten || card2.Rank >= Rank.Ten
                )
                )
            {
                if (_table.AllCards.Count > 0)
                {
                    if (_table.IsFlop)
                    {
                        var c1 = _table.AllCards[0];
                        var c2 = _table.AllCards[1];
                        var c3 = _table.AllCards[2];

                        //Street on flop
                        if(
                            CheckStreetLine(c1, c2, c3, card1, card2)
                            ||
                            CheckStreetLine(card1, card2, c1, c2, c3)
                            ||
                            CheckStreetLine(c1, card1, card2, c2, c3)
                            ||
                            CheckStreetLine(c1, c2, card1, card2, c3)
                            )
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }

                        //Draw Street
                        if (
                            CheckStreetDraw(c1, c2, card1, card2)
                            ||
                            CheckStreetDraw(card1, card2, c1, c2)
                            ||
                            CheckStreetDraw(c1, card1, card2, c2)
                            ||
                            CheckStreetDraw(c1, c3, card1, card2)
                            ||
                            CheckStreetDraw(c1, card1, card2, c3)
                            ||
                            CheckStreetDraw(card1, card2, c1, c3)
                            )
                        {
                            return CheckOrCall();
                        }

                    }
                    if (_table.IsTurn || _table.IsRiver)
                    {
                        var c1 = _table.AllCards[0];
                        var c2 = _table.AllCards[1];
                        var c3 = _table.AllCards[2];
                        var c4 = _table.AllCards[3];

                        //Street
                        if (CheckStreet(c1, c2, c3, c4, card1, card2)
                            ||
                            CheckStreetLine(c1, c2, c3, c4, card1)
                            ||
                            CheckStreetLine(c1, c2, c3, c4, card2)
                            )
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }

                    }
                    //if (_table.IsRiver)
                    //{
                    //    var c1 = _table.AllCards[0];
                    //    var c2 = _table.AllCards[1];
                    //    var c3 = _table.AllCards[2];
                    //    var c4 = _table.AllCards[3];
                    //    var c5 = _table.AllCards[4];
                    //}
                    if (fold)
                        if (_table.ClickCheck())
                            return true;

                    if (fold)
                        _table.ClickFold();
                    return false;
                }
                return CheckOrCall();

                if (fold)
                    if (_table.ClickCheck())
                        return true;

                if (fold)
                    _table.ClickFold();
                return false;

            }
            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }


        /// <summary>
        /// Флэши
        /// </summary>
        /// <param name="fold"></param>
        /// <returns></returns>
        private bool Algo3(bool fold)
        {

            _table.Algo = "algo3[flush]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (card2.Suit == card1.Suit)
            {
                if (_table.AllCards.Count > 0)
                {
                    if (_table.IsFlop)
                    {
                        var c1 = _table.AllCards[0];
                        var c2 = _table.AllCards[1];
                        var c3 = _table.AllCards[2];

                        //Flush
                        if (c1.Suit == c2.Suit && c2.Suit == c3.Suit && c1.Suit == card1.Suit)
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }
                        //half flush
                        if (c2.Suit == c3.Suit && c2.Suit == card1.Suit
                            ||
                            c1.Suit == c2.Suit && c1.Suit == card1.Suit
                            ||
                            c1.Suit == c3.Suit && c1.Suit == card1.Suit
                            )
                        {
                            return CheckOrCall();
                        }

                    }
                    if (_table.IsTurn || _table.IsRiver)
                    {
                        var c1 = _table.AllCards[0];
                        var c2 = _table.AllCards[1];
                        var c3 = _table.AllCards[2];
                        var c4 = _table.AllCards[3];


                        //Flush
                        if (c1.Suit == c2.Suit && c2.Suit == c3.Suit && c1.Suit == card1.Suit)
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }
                        //half flush filled
                        if (c2.Suit == c3.Suit && c2.Suit == card1.Suit && c4.Suit == c2.Suit
                            ||
                            c1.Suit == c2.Suit && c1.Suit == card1.Suit && c4.Suit == c1.Suit
                            ||
                            c1.Suit == c3.Suit && c1.Suit == card1.Suit && c4.Suit == c1.Suit
                            )
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }
                    }
                    //if (_table.IsRiver)
                    //{
                    //    var c1 = _table.AllCards[0];
                    //    var c2 = _table.AllCards[1];
                    //    var c3 = _table.AllCards[2];
                    //    var c4 = _table.AllCards[3];
                    //    var c5 = _table.AllCards[4];
                    //}
                    if (fold)
                        if (_table.ClickCheck())
                            return true;

                    if (fold)
                        _table.ClickFold();
                    return false;
                }
                
                return CheckOrCall();

                if (fold)
                    if (_table.ClickCheck())
                        return true;

                if (fold)
                    _table.ClickFold();
                return false;

            }
            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        
        /// <summary>
        /// Тройки и каре
        /// </summary>
        /// <param name="fold"></param>
        /// <returns></returns>
        private bool Algo4(bool fold)
        {
            _table.Algo = "algo4[3&4]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (card1.Rank >= Rank.Ten && card2.Rank >= Rank.Ten)
            {
                if (_table.AllCards.Count > 0)
                {
                    if (_table.IsFlop || _table.IsTurn || _table.IsRiver)
                    {
                        var c1 = _table.AllCards[0];
                        var c2 = _table.AllCards[1];
                        var c3 = _table.AllCards[2];

                        //Three
                        if (c1.Rank == c2.Rank && c2.Rank == card1.Rank
                            ||
                            c1.Rank == c2.Rank && c2.Rank == card2.Rank
                            ||
                            c2.Rank == c3.Rank && c2.Rank == card1.Rank
                            ||
                            c2.Rank == c3.Rank && c2.Rank == card2.Rank
                            )
                        {
                            if (CurrentCounter <= GetRoundRaiseLimit)
                                return BetOrRaise();
                            else
                                return CheckOrCall();
                        }
						
                    }

                    if (fold)
                        if (_table.ClickCheck())
                            return true;

					if (fold)
					{
						_table.ClickFold();
					}
					return false;
                }
                
                return CheckOrCall();

            }
            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        //*/
        #endregion

        private bool CheckOrFold()
        {
            if (!_table.ClickCheck())
            {
                if (!_table.ClickFold())
                {
                    Ex.Info("Can't check or fold");
                    return false;
                }
            }
            return true;
        }

        
        //private bool Algo2(bool fold)
        //{
        //    _table.Algo = "algo5[any]";

        //    var card1 = (Card)_table.MyCards[0];
        //    var card2 = (Card)_table.MyCards[1];

        //    if(_table.IsPreFlop)
        //    {
        //        if (card1.Rank == card2.Rank && card2.Rank >= Rank.Nine
        //            ||
        //            card1.Rank >= Rank.Jack && card2.Rank >= Rank.Jack && card1.Suit == card2.Suit
        //            )
        //        {
        //            _table.Algo = "algo5[big-pair-or-suit]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();

        //            return NeedBetOrRaise(true);
        //        }

        //        if (card1.Rank == card2.Rank && card2.Rank >= Rank.Two
        //            ||
        //            card1.Rank >= Rank.Nine && card2.Rank >= Rank.Nine && card1.Suit == card2.Suit
        //            )
        //        {
        //            _table.Algo = "algo5[pair-or-suit]";
        //            return CheckOrCall();
        //        }

        //        if (card1.Rank >= Rank.Ten && card2.Rank >= Rank.Ten && !_table.DdAnyRaised)
        //        {
        //            _table.Algo = "algo5[big-card]";
        //            return CheckOrCall();
        //        }

        //        if (!_table.DdAnyRaised && _table.IsBlind)
        //        {
        //            _table.Algo = "algo5[noone-raised]";
        //            return CheckOrCall();
        //        }

        //    }
        //    if (_table.AllCards.Count > 0)
        //    {
        //        var c1 = _table.AllCards[0];
        //        var c2 = _table.AllCards[1];
        //        var c3 = _table.AllCards[2];

        //        var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
        //        var c5 = _table.IsRiver ? _table.AllCards[4] : null;

        //        //Four of a kind
        //        if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[four]";
        //            return NeedBetOrRaise(false);
        //        }

        //        ////Full house
        //        //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
        //        //{
        //        //    _table.Algo = "algo5[house]";
        //        //    return NeedBetOrRaise();
        //        //}

                

        //        //Flush
        //        if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[flush]";
        //            return NeedBetOrRaise(false);
        //        }

        //        //Street
        //        if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[street]";
        //            return NeedBetOrRaise(false);
        //        }

        //        //Triple
        //        if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[triple]";
        //            return NeedBetOrRaise(false);

        //        }

        //        //Two pair
        //        if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[two-pair]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();

        //            return NeedBetOrRaise(false);
        //        }


        //        if (_table.IsFlop)
        //        {
        //            //half Flush
        //            if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-flush]";
        //                return CheckOrCallLimit(fold);
        //            }

        //            //half Street
        //            if (CheckHalfStreet(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-street]";
        //                return CheckOrCallLimit(fold);
        //            }
        //        }

        //        var maxCard = c5 ?? (c4 ?? c3);

        //        if (maxCard.Rank < card2.Rank)
        //            maxCard = card2;

        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
        //        {
        //            _table.Algo = "algo5[over-pair]";
        //            return CheckOrCallLimit(fold);
        //        }
        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two))
        //        {
        //            _table.Algo = "algo5[any-pair]";
        //            if(!_table.IsRiver && !_table.IsTurn)
        //                return CheckOrCallLimit(fold);
        //        }

        //        if (!_table.DdAnyRaised && _table.IsBlind)
        //        {
        //            _table.Algo = "algo5[noone-raised]";
        //            return CheckOrCall();
        //        }
        //    }
        //    if (fold)
        //        if (_table.ClickCheck())
        //            return true;

        //    if (fold)
        //        _table.ClickFold();
        //    return false;
        //}

        //private bool Algo1(bool fold, int rand)
        //{

            
        //    _table.Algo = "a1[any]";

        //    var card1 = (Card)_table.MyCards[0];
        //    var card2 = (Card)_table.MyCards[1];

        //    if (_table.IsPreFlop)
        //    {
        //        #region PreFlop
        //        if (_table.MyCards.HasCards("AA,KK,QQ,JJ") && _table.IsMiddle)
        //        {
        //            _table.Algo = "a1[very-strong]";
        //            if (_table.DdAnyRaised && !_table.IsAllIn)
        //                return CheckOrCall();

        //            if (!_table.DdAnyRaised)
        //            {
        //                if (!_table.IsAllIn)
        //                    return Click3Bet();
        //                else
        //                    return NeedBetOrRaise(true);
        //            }
        //        }

        //        if (_table.MyCards.HasCards("AA,KK,QQ,JJ") && (_table.IsButton || _table.IsAllBlind))
        //        {
        //            _table.Algo = "a1[very-strong]";
        //            if (_table.DdAnyRaised && !_table.IsAllIn)
        //                return CheckOrCall();

        //            if (!_table.DdAnyRaised)
        //            {
        //                if (!_table.IsAllIn)
        //                    return Click3Bet();
        //                else
        //                    return NeedBetOrRaise(true);
        //            }
        //        }

        //        //---
        //        if (_table.MyCards.HasCards("AK,JJ,TT,99,88,AQ,AJ,ATs,AJs") && _table.IsMiddle)
        //        {
        //            _table.Algo = "a1[strong]";
        //            if (_table.DdAnyRaised)
        //            {
        //                if (CurrentCounter == 1 && !_table.IsAllIn)
        //                    return CheckOrCall();
        //            }
        //            else if (!_table.IsAllIn && CurrentCounter == 1)
        //                return NeedBetOrRaise(true);
        //        }

        //        if (_table.MyCards.HasCards("AK,TT,99,88,77,AQ,AJ,AT,A9s,KQ") && _table.IsButton)
        //        {
        //            _table.Algo = "a1[strong]";
        //            if (_table.DdAnyRaised)
        //            {
        //                if (CurrentCounter == 1 && !_table.IsAllIn)
        //                    return CheckOrCall();
        //            }
        //            else if (!_table.IsAllIn && CurrentCounter == 1)
        //                return NeedBetOrRaise(true);
        //        }

        //        if (_table.MyCards.HasCards("AK,TT,99,88,77,AQ,AJ,AT,A9s,KQ") && _table.IsAllBlind)
        //        {
        //            _table.Algo = "a1[strong]";
        //            if (_table.DdAnyRaised)
        //            {
        //                if (!_table.IsAllIn)
        //                    return CheckOrCall();
        //            }
        //            else if (!_table.IsAllIn)
        //                return NeedBetOrRaise(true);
        //        }

        //        if (_table.MyCards.HasCards("AK,AQ,AJ,AT,KQ,KJ,KT,QJ,QT,JT"))
        //        {
        //            _table.Algo = "a1[some-big]";
        //            if (!_table.DdAnyRaised)
        //                return NeedBetOrRaise(true);

        //            if (!_table.IsAllIn && CurrentCounter == 1)
        //            {
        //                return CheckOrCall();
        //            }
        //        }

        //        //steal (button & blind)
        //        if (_table.MyCards.HasCards("99,88,77,66,55,44,33,22,AQ,AJ,AT,A9,A.s") 
        //            && (_table.IsButton || _table.IsAllBlind))
        //        {
        //            _table.Algo = "a1[steal]";
        //            if (!_table.DdAnyRaised)
        //            {
        //                if (!_table.IsAllIn && CurrentCounter == 1)
        //                    return NeedBetOrRaise(true);
        //            }
        //            else //re steal
        //            {
        //                _table.Algo = "a1[steal-re]";
        //                if (CurrentCounter <= 2 && !_table.IsAllIn)
        //                    return NeedBetOrRaise(true);
        //            }
        //        }

        //        if (
        //            (_table.MyCards.HasCards("A.s,K.s,Q.s,J.s") 
        //            ||
        //            card1.Suit == card2.Suit
        //            ||
        //            card2.Rank - card1.Rank == 1
        //            )
        //            && (_table.IsButton || _table.IsAllBlind))
        //        {
        //            _table.Algo = "a1[steal-my]";
        //            if (!_table.DdAnyRaised && !_table.IsAllIn && CurrentCounter == 1)
        //            {
        //                return NeedBetOrRaise(true);
        //            }
        //            if (_table.DdAnyRaised && CurrentCounter <= 2)
        //                return CheckOrCall();
        //        }

        //        if (_table.MyCards.HasCards("A.o,K.o,Q.o") && (_table.IsButton || _table.IsAllBlind))
        //        {
        //            //var rand = _random.Next(0, 100);
        //            if (rand < 70)
        //            {
        //                _table.Algo = "a1[steal-rand70-button]";
        //                if (!_table.DdAnyRaised && !_table.IsAllIn && CurrentCounter == 1)
        //                {
        //                    return NeedBetOrRaise(true);
        //                }
        //                if (rand < 40 && _table.DdAnyRaised && CurrentCounter <=2) //re steal
        //                {
        //                    _table.Algo = "a1[steal-rand70-button-re]";
        //                    return CheckOrCall();
        //                }
        //            }
        //        }

        //        if (!_table.DdAnyRaised && (_table.IsButton || _table.IsAllBlind))
        //        {
        //            //var rand = _random.Next(0, 100);
        //            if (rand < 20 && !_table.IsAllIn && CurrentCounter == 1)
        //            {
        //                _table.Algo = "a1[steal-rand10]";
        //                return NeedBetOrRaise(true);
        //            }
        //            if (rand < 10 && _table.DdAnyRaised && CurrentCounter <= 2) // re steal
        //            {
        //                _table.Algo = "a1[steal-rand10-re]";
        //                return CheckOrCall();
        //            }

        //            if (_table.IsAllBlind && !_table.IsAllIn && CurrentCounter == 1)
        //            {
        //                _table.Algo = "a1[little-blind]";
        //                return CheckOrCall();
        //            }
        //        }
        //        #endregion
        //    }
        //    if (_table.AllCards.Count > 0)
        //    {
        //        var c1 = _table.AllCards[0];
        //        var c2 = _table.AllCards[1];
        //        var c3 = _table.AllCards[2];

        //        var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
        //        var c5 = _table.IsRiver ? _table.AllCards[4] : null;

        //        //Four of a kind
        //        if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "a1[four]";
        //            if (_table.DdAnyRaised && CurrentCounter > 1)
        //                return ClickAllIn();
        //            return NeedBetOrRaise(false);
        //        }

        //        ////Full house
        //        //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
        //        //{
        //        //    _table.Algo = "a1[house]";
        //        //    return NeedBetOrRaise();
        //        //}

        //        //Flush
        //        if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "a1[flush]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();
        //            return NeedBetOrRaise(false);
        //        }

        //        //Street
        //        if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "a1[street]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();
        //            return NeedBetOrRaise(false);
        //        }

        //        //Triple
        //        if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "a1[triple]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();
        //            return NeedBetOrRaise(false);

        //        }

        //        //Two pair
        //        if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "a1[two-pair]";
        //            if (_table.DdAnyRaised)
        //                return CheckOrCall();
        //            return NeedBetOrRaise(false);
        //        }

        //        //All checked
        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two)
        //            && !_table.DdAnyRaised
        //            && _table.IsAllBlind
        //             && !_table.IsAllIn
        //            && CurrentCounter <=2
        //            )
        //        {
        //            _table.Algo = "a1[pair-all-checked]";
        //            return NeedBetOrRaise(false);
        //        }

        //        //over-pair
        //        var maxCard = c5 ?? (c4 ?? c3);
        //        if (maxCard.Rank < card2.Rank)
        //        {
        //            maxCard = card2;
        //        }
        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
        //        {
        //            _table.Algo = "a1[over-pair]";

        //            if (!_table.IsFreePlay)
        //            {
        //                if (!_table.IsAllIn)
        //                    return CheckOrCall();
        //                //if (_table.DdAnyRaised)
        //                //    return ClickAllIn();
        //                //return NeedBetOrRaise(false);
        //            }
        //        }

        //        if (_table.IsFlop || _table.IsTurn)
        //        {
        //            //monstr-draw
        //            maxCard = c5 ?? (c4 ?? c3);
        //            if (
        //                (
        //                    CheckHalfFlush(c1, c2, c3, c4, card1, card2)
        //                    &&
        //                    CheckHalfStreet(c1, c2, c3, c4, card1, card2)
        //                )
        //                ||
        //                (
        //                    CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank)
        //                    &&
        //                    CheckHalfFlush(c1, c2, c3, c4, card1, card2)
        //                )
        //                )
        //            {
        //                _table.Algo = "a1[monstr-draw]";
        //                if (!_table.IsAllIn)
        //                    return CheckOrCall();
        //                //if (_table.DdAnyRaised)
        //                //    return ClickAllIn();
        //                //return NeedBetOrRaise(false);
        //            }

        //            //top-pair
        //            maxCard = c5 ?? (c4 ?? c3);
        //            if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
        //            {
        //                _table.Algo = "a1[top-pair]";

        //                if (!_table.IsFreePlay)
        //                {
        //                    if (!_table.DdAnyRaised && !_table.IsAllIn)
        //                        return NeedBetOrRaise(false);
        //                    //else fold :)
        //                }
        //            }
        //        }

        //        if (_table.IsFlop)
        //        {
        //            //half Flush
        //            if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "a1[half-flush]";
        //                if (!_table.IsFreePlay)
        //                {
        //                    if (!_table.IsAllIn)
        //                        return CheckOrCall();
        //                    //if (_table.DdAnyRaised)
        //                    //    return ClickAllIn();
        //                    //return NeedBetOrRaise(false);
        //                }
        //            }

        //            //half Street
        //            if (CheckHalfStreet(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "a1[half-street]";
        //                if (!_table.IsFreePlay)
        //                {
        //                    if (!_table.IsAllIn)
        //                        return CheckOrCall();
        //                    //if (_table.DdAnyRaised)
        //                    //    return ClickAllIn();
        //                    //return NeedBetOrRaise(false);
        //                }
        //            }

        //            bool isDrawBoard = CheckDrawBoard(c1, c2, c3) || _table.ActivePlayers > 2;

        //            //if trash hand
        //            if (!isDrawBoard)
        //            {
        //                if (!_table.IsFreePlay)
        //                {
        //                    _table.Algo = "a1[trash-hand]";
        //                    if (!_table.DdAnyRaised && !_table.IsAllIn)
        //                        return NeedBetOrRaise(false);
        //                }
        //            }
        //        }

        //        //steal
        //        if (!_table.DdAnyRaised && _table.IsAllBlind)
        //        {
        //            _table.Algo = "a1[steal-post-flop]";
        //            var val = _random.Next(0, 100);
        //            if (val < 50 && !_table.IsAllIn)
        //                return NeedBetOrRaise(false);
        //        }

        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two))
        //        {
        //            _table.Algo = "a1[any-pair]";
        //            //if (_table.LimitByBet && CurrentCounter == 1)
        //            if (CurrentCounter == 1)
        //                return CheckOrCall();
                    
        //        }
        //    }
        //    if (fold)
        //        if (_table.ClickCheck())
        //            return true;

        //    if (fold)
        //        _table.ClickFold();
        //    return false;
        //}

        private bool Algo2(bool fold, int rand, int bluff)
        {


            _table.Algo = "a2[any]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (_table.IsPreFlop)
            {
                #region PreFlop

                #region main opening

                //============= Early ================
                if (_table.MyCards.HasCards("AA,KK,QQ") && _table.IsEarly)
                {
                    _table.Algo = "a2[very-strong]";
                    
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                    else
                    {
                        if (CurrentCounter == 1)
                            return Click3Bet();
                        return ClickAllIn();
                    }
                }

                if (_table.MyCards.HasCards("JJ,TT,AK,AQ,AJs") && _table.IsEarly)
                {
                    _table.Algo = "a2[strong]";
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                }
                
                //================ Middle =================
                if (_table.MyCards.HasCards("AA,KK,QQ,AK") && _table.IsMiddle)
                {
                    _table.Algo = "a2[very-strong]";

                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                    else
                    {
                        if (CurrentCounter == 1)
                            return Click3Bet();
                        return ClickAllIn();
                    }
                }

                if (_table.MyCards.HasCards("JJ,TT,99,88,AQ,AJ,ATs") && _table.IsMiddle)
                {
                    _table.Algo = "a2[strong]";
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                }

                //================ Blind/Cutoff/Button =================
                if (_table.MyCards.HasCards("AA,KK,QQ,JJ,AK") && _table.IsLate)
                {
                    _table.Algo = "a2[very-strong]";

                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                    else
                    {
                        if (CurrentCounter == 1)
                            return Click3Bet();
                        return ClickAllIn();
                    }
                }

                if (_table.MyCards.HasCards("TT,99,88,77,AQ,AJ,AT,A9s,KQ") && _table.IsLate)
                {
                    _table.Algo = "a2[strong]";
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                }

                #endregion

                #region Steals chart
                //steal then NoRaise or CurrentCounter >= 2

                // steal
                if (_table.MyCards.HasCards("AA,KK,QQ,JJ,TT,AK") && _table.IsLate)
                {
                    _table.Algo = "a2[steal]";
                    
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                    else if (CurrentCounter >= 2)
                    {
                        _table.Algo = "a2[re-steal]";
                        return ClickAllIn();
                    }

                    //return NeedBetOrRaise(true);
                }

                if (_table.MyCards.HasCards("99,88,77,66,55,AQ,AJ,AT,A9s,KQ,KJs,QJs") && _table.IsCutOff)
                {
                    _table.Algo = "a2[steal]";
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                }

                
                //steal
                if (_table.MyCards.HasCards("99,88,77,66,55,44,33,22,AQ,AJ,AT,A.s,KQ,KJ,KT,QJ,QT,JT") 
                    && (_table.IsSmallBlind || _table.IsButton))
                {
                    _table.Algo = "a2[steal]";
                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                }

                // single re-steal
                if (_table.MyCards.HasCards("99,88,AQ,AJ") && _table.IsBlind)
                {
                    _table.Algo = "a2[steal]";

                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return Click3Bet();
                    }
                    else if (CurrentCounter == 2)
                    {
                        _table.Algo = "a2[re-steal]";
                        return Click3Bet();
                    }
                }
                #endregion

                #region 3-bet chart

                if (_table.RaiserCount == 1 && !_table.NoRaise)
                {
                    if (_table.IsRaiserMiddle && _table.MyCards.HasCards("JJ,TT,99,AQ,AJ,ATs"))
                    {
                        _table.Algo = "a2[3-bet]";
                        if (CurrentCounter == 1)
                        {
                            return Click3Bet();
                        }
                    }

                    if (_table.IsRaiserCoSbBu && _table.MyCards.HasCards("TT,99,88,AQ,AJ,AT,A9s"))
                    {
                        _table.Algo = "a2[3-bet]";
                        if (CurrentCounter == 1)
                        {
                            return Click3Bet();
                        }
                    }
                }

                #endregion

                #endregion
            }
            if (_table.AllCards.Count > 0)
            {
                var c1 = _table.AllCards[0];
                var c2 = _table.AllCards[1];
                var c3 = _table.AllCards[2];

                var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
                var c5 = _table.IsRiver ? _table.AllCards[4] : null;

                //Four of a kind
                if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a2[four]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                        //return NeedBetOrRaise(true);
                    return ClickAllIn();
                }

                ////Full house
                //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
                //{
                //    _table.Algo = "a2[house]";
                //    return NeedBetOrRaise();
                //}

                //Flush
                if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a2[flush]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                        //return NeedBetOrRaise(true);
                    return ClickAllIn();
                }

                //Street
                if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a2[street]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                        //return NeedBetOrRaise(true);
                    return ClickAllIn();
                }

                //Triple
                if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a2[triple]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                        //return NeedBetOrRaise(true);
                    return ClickAllIn();

                }

                //Two pair
                if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a2[two-pair]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                        //return NeedBetOrRaise(true);
                    return ClickAllIn();
                }

                //over-pair
                var maxCard = c5 ?? (c4 ?? c3);
                if (card1.Rank == card2.Rank && card1.Rank >= maxCard.Rank)
                {
                    _table.Algo = "a2[over-pair]";
                    if (_table.NoRaise)
                        return ClickOneHalf();
                    return ClickAllIn();
                }


                if (_table.IsFlop)
                {
                    //half Flush
                    if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
                    {
                        _table.Algo = "a2[half-flush]";
                        if (_table.NoRaise)
                            return ClickOneHalf();
                    }

                    //oesd Street
                    if (CheckOesdStreet(c1, c2, c3, c4, card1, card2))
                    {
                        _table.Algo = "a2[oesd-street]";
                        if (_table.NoRaise)
                            return ClickOneHalf();
                    }
                }

                if (_table.IsFlop || _table.IsTurn)
                {
                    //monstr-draw
                    maxCard = c5 ?? (c4 ?? c3);
                    if (
                        (
                            CheckHalfFlush(c1, c2, c3, c4, card1, card2)
                            &&
                            CheckHalfStreet(c1, c2, c3, c4, card1, card2)
                        )
                        ||
                        (
                            CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank)
                            &&
                            CheckHalfFlush(c1, c2, c3, c4, card1, card2)
                        )
                        )
                    {
                        _table.Algo = "a2[monstr-draw]";
                        if (_table.NoRaise)
                            return ClickOneHalf();
                        return ClickAllIn();
                    }

                    //top-pair
                    maxCard = c5 ?? (c4 ?? c3);
                    if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
                    {
                        _table.Algo = "a2[top-pair]";
                        if (_table.NoRaise)
                            return ClickOneHalf();
                    }
                }

                if (_table.IsFlop)
                {

                    if (_table.IsInitiative && CurrentCounter == 1 && !_table.IsAllIn && _table.NoRaise)
                    {
                        _table.Algo = "a2[cont-bet]";
                        return ClickOneHalf();
                    }

                    //maxCard = c5 ?? (c4 ?? c3);
                    //if(_table.IsInitiative
                    //    && CurrentCounter == 2
                    //    && (
                    //        CheckMiddlePair(c1, c2, c3, card1, card2) 
                    //        || _table.MyCards.HasCards("AK,AQ")
                    //        || CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank) //check top pair
                    //        )
                    //    )
                    //{
                    //    _table.Algo = "a2[cont-re-bet]";
                    //    return ClickOneHalf();
                    //}

                    bool isDrawBoard = CheckDrawBoard(c1, c2, c3) || _table.ActivePlayers > 2;

                    //if trash hand
                    if (!isDrawBoard)
                    {
                        //half Street
                        if (CheckHalfStreet(c1, c2, c3, c4, card1, card2))
                        {
                            _table.Algo = "a2[half-street]";
                            if (_table.NoRaise)
                                return ClickOneHalf();
                        }

                        _table.Algo = "a2[no-draw]";
                        if(_table.NoRaise)
                            return ClickOneHalf();
                    }
                }

                if (_table.IsRiver)
                {
                    //top-pair
                    maxCard = c5 ?? (c4 ?? c3);
                    if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
                    {
                        _table.Algo = "a2[top-pair]";
                        if (_table.NoRaise)
                            return ClickOneHalf();
                        return ClickAllIn();
                    }
                }
            }

            if (bluff < _settings.BluffPercent)
            {
                return AlgoBluff(true);
            }

            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        bool IsAllCalledOnPrevRound()
        {
            return (!_table.IsFreePlay
                    && PrevCounter == 2
                    && (_table.IsFlop || _table.IsTurn || _table.IsRiver)
                   ); //All called on prev round
        }

        private bool Algo1(bool fold, int rand, int bluff)
        {


            _table.Algo = "a1[any]";

            var card1 = (Card)_table.MyCards[0];
            var card2 = (Card)_table.MyCards[1];

            if (_table.IsPreFlop)
            {
                #region PreFlop

                #region main opening

                if (_table.NoRaise)
                {
                    if (_table.MyCards.HasCards("AA,KK,QQ,JJ,AK") && _table.IsEarly)
                    {
                        _table.Algo = "a1[strong]";
                        _table.IsInitiative = true;
                        return ClickOneHalf();
                    }
                    if (_table.MyCards.HasCards("AA,KK,QQ,JJ,TT,99,AK,AQ") && _table.IsMiddle)
                    {
                        _table.Algo = "a1[strong]";
                        _table.IsInitiative = true;
                        return ClickOneHalf();
                    }
                    if (_table.MyCards.HasCards("AA,KK,QQ,JJ,TT,99,88,77,66,AT,AJ,AQ,AK,KQ") && _table.IsLate)
                    {
                        _table.Algo = "a1[strong]";
                        _table.IsInitiative = true;
                        return ClickOneHalf();
                    }
                }
                else
                {
                    if (_table.IsInitiative && _table.MyCards.HasCards("AA,KK,QQ,JJ,TT,AK"))
                    {
                        _table.Algo = "a1[strong]";
                        return ClickAllIn();
                    }
                    if (!_table.IsInitiative && _table.MyCards.HasCards("AA,KK"))
                    {
                        _table.Algo = "a1[strong]";
                        return ClickAllIn();
                    }

                    if (_table.IsInitiative && _table.IsLate && _table.MyCards.HasCards("AA,KK,QQ,JJ,TT,99,88,AJ,AQ,AK"))
                    {
                        _table.Algo = "a1[re-steal]";
                        return ClickAllIn();
                    }
                }
                
                #endregion

                #endregion
            }
            if (_table.AllCards.Count > 0)
            {

                if (_table.Bank.BankValue >= _table.MyBalance.Posa*2
                    && _table.MyBalance.IsValid
                    && _table.Bank.BankValueValid
                    && !_table.IsFreePlay
                    )
                {
                    _table.Algo = "a1[no-return]";
                    return ClickAllIn();
                }

                var c1 = _table.AllCards[0];
                var c2 = _table.AllCards[1];
                var c3 = _table.AllCards[2];

                var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
                var c5 = _table.IsRiver ? _table.AllCards[4] : null;


                ////Full house
                //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
                //{
                //    _table.Algo = "a2[house]";
                //    return NeedBetOrRaise();
                //}

                //Four of a kind
                if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a1[four]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                //Flush
                if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a1[flush]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                //Street
                if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a1[street]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                //Triple
                if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a1[triple]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();

                }

                //Two pair
                if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
                {
                    _table.Algo = "a1[two-pair]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                //over-pair
                var maxCard = c5 ?? (c4 ?? c3);
                if (card1.Rank == card2.Rank && card1.Rank >= maxCard.Rank)
                {
                    _table.Algo = "a1[over-pair]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                //top-pair
                maxCard = c5 ?? (c4 ?? c3);
                if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank) 
                    && _table.IsFreePlay 
                    && (card1.Rank >= Rank.Jack || card2.Rank >= Rank.Jack)
                    )
                {
                    _table.Algo = "a1[top-pair]";
                    if (_table.NoRaise)
                    {
                        if (IsAllCalledOnPrevRound()) return ClickAllIn();
                        return Click23();
                    }
                    return ClickAllIn();
                }

                if (!_table.IsFreePlay)
                {

                    //top-pair
                    maxCard = c5 ?? (c4 ?? c3);
                    if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
                    {
                        _table.Algo = "a1[top-pair]";
                        if (_table.NoRaise)
                        {
                            if (IsAllCalledOnPrevRound()) return ClickAllIn();
                            return Click23();
                        }
                        return ClickAllIn();
                    }

                    //middle-pair
                    Card midCard = null;
                    if (_table.IsFlop)
                        midCard = c2;
                    if (_table.IsTurn)
                        midCard = c3;
                    if (_table.IsRiver)
                        midCard = c4;

                    maxCard = c5 ?? (c4 ?? c3);
                    if (card1.Rank == card2.Rank && card1.Rank >= midCard.Rank && maxCard.Rank != Rank.Ace)
                    {
                        _table.Algo = "a1[middle-pair]";
                        if (_table.NoRaise)
                        {
                            if (IsAllCalledOnPrevRound()) return ClickAllIn();
                            return Click23();
                        }
                        return ClickAllIn();
                    }

                    //half Flush
                    if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
                    {
                        _table.Algo = "a1[half-flush]";
                        if (_table.NoRaise)
                        {
                            if (IsAllCalledOnPrevRound()) return ClickAllIn();
                            return Click23();
                        }
                        return ClickAllIn();
                    }

                    //oesd Street
                    if (CheckOesdStreet(c1, c2, c3, c4, card1, card2))
                    {
                        _table.Algo = "a1[oesd-street]";
                        if (_table.NoRaise)
                        {
                            if (IsAllCalledOnPrevRound()) return ClickAllIn();
                            return Click23();
                        }
                        return ClickAllIn();
                    }



                    //monstr-draw
                    maxCard = c5 ?? (c4 ?? c3);
                    if (
                        (
                            CheckHalfFlush(c1, c2, c3, c4, card1, card2)
                            &&
                            CheckHalfStreet(c1, c2, c3, c4, card1, card2)
                        )
                        ||
                        (
                            CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank)
                            &&
                            CheckHalfFlush(c1, c2, c3, c4, card1, card2)
                        )
                        )
                    {
                        _table.Algo = "a1[monstr-draw]";
                        if (_table.NoRaise)
                        {
                            if (IsAllCalledOnPrevRound()) return ClickAllIn();
                            return Click23();
                        }
                        return ClickAllIn();
                    }

                }
            }

            if (bluff < _settings.BluffPercent)
            {
                return AlgoBluff(true);
            }

            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        private bool Click23()
        {
            _table.ClickButton3();
            Thread.Sleep(100);
            if (!_table.ClickBet(true))
                if (!_table.ClickRaise(true))
                    if (!_table.ClickCall(true))
                        if (!_table.ClickCheck())
                        {
                            //Ex.Info("Can't bet or raise");
                            //return false;
                            return _table.ClickFold();
                        }

            return true;
        }

        private bool AlgoBluff(bool fold)
        {
            //================ Blind/Cutoff/Button =================
            if (_table.IsPreFlop)
            {
                if (_table.IsLate)
                {
                    _table.Algo = "bluff[very-strong]";

                    if (_table.NoRaise)
                    {
                        _table.IsInitiative = true;
                        return ClickOneHalf();
                    }
                    else if(_table.IsInitiative)
                    {
                        if (CurrentCounter <= 2)
                            return ClickOneHalf();
                        return ClickAllIn();
                    }
                }
            }

            if (_table.AllCards.Count > 0)
            {
                var c1 = _table.AllCards[0];
                var c2 = _table.AllCards[1];
                var c3 = _table.AllCards[2];

                var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
                var c5 = _table.IsRiver ? _table.AllCards[4] : null;

                if (_table.IsFlop || _table.IsTurn)
                {
                    if (CheckDrawBoard(c1, c2, c3, c4) || _table.IsInitiative)
                    {
                        _table.Algo = "bluff";
                        if (_table.NoRaise)
                        {
                            return ClickOneHalf();
                        }
                        else
                        {
                            if (CurrentCounter <= 2)
                                return ClickOneHalf();
                            return ClickAllIn();
                        }
                    }
                }

                if (_table.IsRiver)
                {
                    _table.Algo = "bluff[river]";
                    if (_table.NoRaise)
                    {
                        return ClickOneHalf();
                    }
                    else
                    {
                        if (CurrentCounter <= 2)
                            return ClickOneHalf();
                        return ClickAllIn();
                    }
                }
            }

            if (fold)
                if (_table.ClickCheck())
                    return true;

            if (fold)
                _table.ClickFold();
            return false;
        }

        Random _bluffRandom = new Random();

        private bool CheckMiddlePair(Card c1, Card c2, Card c3, Card card1, Card card2)
        {

            return CheckPair(c1, c2, c3, null, null, card1, card2, c2.Rank)
                ||
                card1.Rank == card2.Rank && card1.Rank >= c2.Rank
                ;
        }

        /// <summary>
        /// Дровяной борд
        /// </summary>
        /// <returns></returns>
        bool CheckDrawBoard(Card c1, Card c2, Card c3)
        {
            if (CheckStreetAlgo(new[] {c1, c2, c3}, 3))
                return true;

            if (
                (c3.Rank - c2.Rank == 1 || c2.Rank - c1.Rank == 1)
                &&
                (c1.Suit == c2.Suit || c2.Suit == c3.Suit || c1.Suit == c3.Suit)
                )
                return true;

            if (c1.Suit == c2.Suit && c2.Suit == c3.Suit)
                return true;

            return false;
        }


        bool CheckDrawBoard(Card c1, Card c2, Card c3, Card c4)
        {
            if (c4 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3 });

                if (lsAll.Any(obj => obj.Value >= 2 && obj.Key >= c3.Rank))
                    return true;

                if (lsAll.Any(obj => obj.Value >= 2 && obj.Key >= c2.Rank))
                    return true;

                if (c1.Suit == c2.Suit && c2.Suit == c3.Suit
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c3 }, 3)
                    ||
                    c1.Rank == c2.Rank && c2.Rank == c3.Rank
                    )
                    return true;
            }

            if (c4 != null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4 });

                if (lsAll.Any(obj => obj.Value >= 2 && obj.Key >= c4.Rank))
                    return true;

                if (lsAll.Any(obj => obj.Value >= 2 && obj.Key >= c3.Rank))
                    return true;

                if (c1.Suit == c2.Suit && c2.Suit == c4.Suit
                    ||
                    c1.Suit == c4.Suit && c4.Suit == c3.Suit
                    ||
                    c2.Suit == c3.Suit && c3.Suit == c4.Suit

                    ||

                    CheckStreetAlgo(new[] { c1, c2, c3 }, 3)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c4 }, 3)
                    ||
                    CheckStreetAlgo(new[] { c1, c3, c4 }, 3)
                    ||
                    CheckStreetAlgo(new[] { c2, c3, c4 }, 3)
                    )
                    return true;
            }

            return false;
        }

        private bool ClickOneHalf()
        {
            _table.ClickButton2();
            Thread.Sleep(100);
            if (!_table.ClickBet(true))
                if (!_table.ClickRaise(true))
                    if (!_table.ClickCall(true))
                        if (!_table.ClickCheck())
                        {
                            //Ex.Info("Can't bet or raise");
                            //return false;
                            return _table.ClickFold();
                        }

            return true;
        }

        bool Click3Bet()
        {
            _table.ClickButton2();
            Thread.Sleep(100);
            //_table.ClickButton2();
            return BetOrRaise(true);
        }

        
        bool ClickAllIn()
        {
            //TODO useless limit check
            //var val = _settings.BuyIn;
            //if (_table.MyBalance.IsValid)
            //{
            //    if (_table.MyBalance.Posa >= val)
            //    {
            //        _table.ClickBet(val);
            //    }
            //    else
            //    {
            //        _table.ClickButton4();
            //    }
            //}
            _table.ClickButton4();

            Thread.Sleep(100);
            //_table.ClickButton4();
            return BetOrRaise(true);
        }

        //private bool Algo3(bool fold)
        //{
        //    _table.Algo = "algo5[any]";

        //    var card1 = (Card)_table.MyCards[0];
        //    var card2 = (Card)_table.MyCards[1];

        //    if (_table.IsPreFlop)
        //    {
        //        if (card1.Rank == card2.Rank && card2.Rank >= Rank.Queen
        //            ||
        //            card1.Rank >= Rank.King && card2.Rank >= Rank.King
        //            )
        //        {
        //            _table.Algo = "algo5[very-strong]";
        //            return NeedBetOrRaise();
        //        }

        //        if (card1.Rank == card2.Rank && card2.Rank >= Rank.Nine
        //            ||
        //            card1.Rank >= Rank.Jack && card2.Rank == Rank.Ace
        //            )
        //        {
        //            _table.Algo = "algo5[strong]";
        //            if (_table.MyPosition == Table.TablePosition.Blind || _table.MyPosition == Table.TablePosition.Button)
        //            {
        //                return NeedBetOrRaise();
        //            }
        //            return CheckOrCall();
        //        }
                
        //    }
        //    if (_table.AllCards.Count > 0)
        //    {
        //        var c1 = _table.AllCards[0];
        //        var c2 = _table.AllCards[1];
        //        var c3 = _table.AllCards[2];

        //        var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
        //        var c5 = _table.IsRiver ? _table.AllCards[4] : null;

        //        //Four of a kind
        //        if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[four]";
        //            return NeedBetOrRaise();
        //        }

        //        ////Full house
        //        //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
        //        //{
        //        //    _table.Algo = "algo5[house]";
        //        //    return NeedBetOrRaise();
        //        //}



        //        //Flush
        //        if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[flush]";
        //            return NeedBetOrRaise();
        //        }

        //        //Street
        //        if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[street]";
        //            return NeedBetOrRaise();
        //        }

        //        //Triple
        //        if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[triple]";
        //            return NeedBetOrRaise();

        //        }

        //        //Two pair
        //        if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[two-pair]";
        //            return NeedBetOrRaise();
        //        }


        //        if (_table.IsFlop || _table.IsTurn)
        //        {
        //            //half Flush
        //            if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-flush]";
        //                if (CurrentCounter == 1)
        //                    return RaiseOrBetOrCallOrCheck();
        //                return CheckOrCallLimit(fold);
        //                //if (CurrentCounter <= GetRoundCallLimit)
        //                //    return CheckOrCall();
        //            }

        //            //half Street
        //            if (CheckHalfStreet(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-street]";
        //                if (CurrentCounter == 1)
        //                    return RaiseOrBetOrCallOrCheck();
        //                return CheckOrCallLimit(fold);
        //                //if (CurrentCounter <= GetRoundCallLimit)
        //                //    return CheckOrCall();
        //            }
        //        }

        //        var maxCard = c5 != null ? c5 : (c4 != null ? c4 : c3);

        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Jack))
        //        {
        //            _table.Algo = "algo5[big-pair]";
        //            return CheckOrCallLimit(fold);
        //        }
        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two))
        //        {
        //            _table.Algo = "algo5[any-pair]";
        //            return CheckOrCallLimit(fold);
        //        }
        //        ////any pair
        //        //if(
        //        //    (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two) && _table.AnyPair)
        //        //    ||
        //        //    (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Jack) && !_table.AnyPair)
        //        //    )
        //        //{
        //        //    if (_table.AnyPair)
        //        //        _table.Algo = "algo5[any-pair]";
        //        //    else
        //        //        _table.Algo = "algo5[pair]";

        //        //    if (CurrentCounter == 1)
        //        //        return BetOrCallOrCheck();
        //        //    return CheckOrCallLimit(fold);
        //        //    //if (CurrentCounter <= GetRoundCallLimit)
        //        //    //    return CheckOrCall();

        //        //}
        //    }
        //    if (fold)
        //        if (_table.ClickCheck())
        //            return true;

        //    if (fold)
        //        _table.ClickFold();
        //    return false;
        //}
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="fold"></param>
        /// <returns></returns>
        //private bool Algo2(bool fold)
        //{
        //    _table.Algo = "algo5[any]";

        //    var card1 = (Card)_table.MyCards[0];
        //    var card2 = (Card)_table.MyCards[1];

        //    if (_table.IsPreFlop)
        //    {
        //        if (_table.MyCards.HasCards("AA,KK,QQ,AKs,AKo"))
        //        {
        //            _table.Algo = "algo5[very-strong]";
        //            return NeedBetOrRaise();
        //        }

        //        if (_table.MyCards.HasCards("JJ,TT,99,AQs,AQo,AJs"))
        //        {
        //            _table.Algo = "algo5[strong]";
        //            if (_table.DdAllCheckOrFold || _table.DdAllCalled || (_table.DdOneRaisedAllFold && !_table.IsMiddle))
        //            {
        //                return NeedBetOrRaise();
        //            }
        //            return CheckOrCall();
        //        }

        //        if (_table.MyCards.HasCards("AJo,ATs,ATo,KQs,KQo"))
        //        {
        //            _table.Algo = "algo5[mid-strong]";
        //            if (!_table.IsMiddle && (_table.DdAllCheckOrFold || _table.DdAllCalled))
        //            {
        //                return NeedBetOrRaise();
        //            }
        //            if (_table.IsBlind && (_table.DdOneRaisedAllFold || _table.DdOneRaisedOneCalled))
        //            {
        //                return CheckOrCall();
        //            }
        //        }

        //        if (_table.MyCards.HasCards("88,77,66,55,44,33,22,KJs,KTs,QJs,QTs,JTs,T9s"))
        //        {
        //            _table.Algo = "algo5[strong-specul]";
        //            if (!_table.IsMiddle && (_table.DdAllCheckOrFold || _table.DdAllCalled))
        //            {
        //                return CheckOrCall();
        //            }
        //            if (_table.DdOneRaisedOneCalled)
        //            {
        //                return CheckOrCall();
        //            }
        //            if (_table.IsBlind && _table.DdOneRaisedAllFold)
        //            {
        //                return CheckOrCall();
        //            }
        //        }

        //        if (_table.MyCards.HasCards("KJo,KTo,QJo,QTo,JTo,A9s,A8s,A7s,A6s,A5s,A4s,A3s,A2s,K9s,87s,98s"))
        //        {
        //            _table.Algo = "algo5[mixed]";
        //            if (!_table.IsMiddle && (_table.DdAllCheckOrFold || _table.DdAllCalled))
        //            {
        //                return CheckOrCall();
        //            }
                    
        //        }
        //    }
        //    if (_table.AllCards.Count > 0)
        //    {
        //        var c1 = _table.AllCards[0];
        //        var c2 = _table.AllCards[1];
        //        var c3 = _table.AllCards[2];

        //        var c4 = (_table.IsTurn || _table.IsRiver) ? _table.AllCards[3] : null;
        //        var c5 = _table.IsRiver ? _table.AllCards[4] : null;

        //        //Four of a kind
        //        if (CheckFour(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[four]";
        //            return NeedBetOrRaise();
        //        }

        //        ////Full house
        //        //if (CheckAllHouse(c1, c2, c3, c4, c5, card1, card2))
        //        //{
        //        //    _table.Algo = "algo5[house]";
        //        //    return NeedBetOrRaise();
        //        //}



        //        //Flush
        //        if (CheckFlush(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[flush]";
        //            return NeedBetOrRaise();
        //        }

        //        //Street
        //        if (CheckStreet(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[street]";
        //            return NeedBetOrRaise();
        //        }

        //        //Triple
        //        if (CheckTriple(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[triple]";
        //            return NeedBetOrRaise();

        //        }

        //        //Two pair
        //        if (CheckTwoPair(c1, c2, c3, c4, c5, card1, card2))
        //        {
        //            _table.Algo = "algo5[two-pair]";
        //            return NeedBetOrRaise();
        //        }


        //        if (_table.IsFlop || _table.IsTurn)
        //        {
        //            //half Flush
        //            if (CheckHalfFlush(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-flush]";

        //                //if (CurrentCounter == 1 && _table.IsFlop)
        //                //    return RaiseOrBetOrCallOrCheck();

        //                return CheckOrCallLimit(fold);
        //            }

        //            //half Street
        //            if (CheckHalfStreet(c1, c2, c3, c4, card1, card2))
        //            {
        //                _table.Algo = "algo5[half-street]";

        //                //if (CurrentCounter == 1 && _table.IsFlop)
        //                //    return RaiseOrBetOrCallOrCheck();

        //                return CheckOrCallLimit(fold);
        //            }
        //        }

        //        var maxCard = c5 ?? (c4 ?? c3);
        //        if (maxCard.Rank < card2.Rank)
        //        {
        //            maxCard = card2;
        //        }

        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, maxCard.Rank))
        //        {
        //            _table.Algo = "algo5[over-pair]";

        //            if (CurrentCounter == 1 && _table.IsFlop)
        //                return NeedBetOrRaise();

        //            return CheckOrCallLimit(fold);
        //        }
        //        if (CheckPair(c1, c2, c3, c4, c5, card1, card2, Rank.Two))
        //        {
        //            _table.Algo = "algo5[any-pair]";
        //            return CheckOrCallLimit(fold);
        //        }
        //    }
        //    if (fold)
        //        if (_table.ClickCheck())
        //            return true;

        //    if (fold)
        //        _table.ClickFold();
        //    return false;
        //}

        private bool ValidateAnyPair(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2, Card maxCard)
        {
            if(_table.IsPreFlop)
            {
                return card1.Rank == card2.Rank;
            }
            if(_table.IsFlop)
            {
                return 
                    (card1.Rank == card2.Rank)
                    || 
                    (c3.Rank == card1.Rank)
                    || 
                    (c3.Rank == card2.Rank)

                    ||
                    (c2.Rank == card1.Rank)
                    ||
                    (c2.Rank == card2.Rank)

                    ||
                    (c1.Rank == card1.Rank)
                    ||
                    (c1.Rank == card2.Rank);
            }
            if(_table.IsTurn)
            {
                return
                    (card1.Rank == card2.Rank)
                    || 
                    (c3.Rank == card1.Rank)
                    || 
                    (c3.Rank == card2.Rank)

                    ||
                    (c2.Rank == card1.Rank)
                    ||
                    (c2.Rank == card2.Rank)

                    ||
                    (c1.Rank == card1.Rank)
                    ||
                    (c1.Rank == card2.Rank)
                    ||
                    (c4.Rank == card1.Rank)
                    ||
                    (c4.Rank == card2.Rank);
            }
            if(_table.IsRiver)
            {
                return (card1.Rank == card2.Rank)
                    ||
                    (c3.Rank == card1.Rank)
                    ||
                    (c3.Rank == card2.Rank)

                    ||
                    (c2.Rank == card1.Rank)
                    ||
                    (c2.Rank == card2.Rank)

                    ||
                    (c1.Rank == card1.Rank)
                    ||
                    (c1.Rank == card2.Rank)
                    ||
                    (c5.Rank == card1.Rank)
                    ||
                    (c5.Rank == card2.Rank);
            }
            //(card1.Rank == card2.Rank)
                    //|| (c1.Rank == card1.Rank)
                    //|| (c1.Rank == card2.Rank)
                    //|| (c2.Rank == card1.Rank)
                    //|| (c2.Rank == card2.Rank)
                    //|| (c3.Rank == card1.Rank)
                    //|| (c3.Rank == card2.Rank)
                    //|| (c4 != null && (
                    //    (c4.Rank == card1.Rank)
                    //    || (c4.Rank == card2.Rank)
                    //    ))
                    //|| (c5 != null && (
                    //    (c5.Rank == card1.Rank)
                    //    || (c5.Rank == card2.Rank)
                    //    ))

                    //(card1.Rank == card2.Rank && card1.Rank >= maxCard.Rank)
                    //|| (c1.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
                    //|| (c1.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
                    //|| (c2.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
                    //|| (c2.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
                    //|| (c3.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
                    //|| (c3.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
                    //|| (c4 != null && (
                    //    (c4.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
                    //    || (c4.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
                    //    ))
                    //|| (c5 != null && (
                    //    (c5.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
                    //    || (c5.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
                    //    ))
            return false;
        }

        private bool ValidatePair(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2, Card maxCard)
        {
            if (_table.IsPreFlop)
            {
                return card1.Rank == card2.Rank;
            }
            if (_table.IsFlop)
            {
                return
                    (card1.Rank == card2.Rank && card2.Rank >= c3.Rank)
                    ||
                    (c3.Rank == card1.Rank)
                    ||
                    (c3.Rank == card2.Rank)

                    ||
                    (c2.Rank == card1.Rank)
                    ||
                    (c2.Rank == card2.Rank)

                    ||
                    (c1.Rank == card1.Rank)
                    ||
                    (c1.Rank == card2.Rank);
            }
            if (_table.IsTurn)
            {
                return
                    (card1.Rank == card2.Rank && card2.Rank >= c4.Rank)
                    ||
                    (c4.Rank == card1.Rank)
                    ||
                    (c4.Rank == card2.Rank);
            }
            if (_table.IsRiver)
            {
                return (card1.Rank == card2.Rank && card2.Rank >= c5.Rank)
                    ||
                    (c5.Rank == card1.Rank)
                    ||
                    (c5.Rank == card2.Rank);
            }
            //(card1.Rank == card2.Rank)
            //|| (c1.Rank == card1.Rank)
            //|| (c1.Rank == card2.Rank)
            //|| (c2.Rank == card1.Rank)
            //|| (c2.Rank == card2.Rank)
            //|| (c3.Rank == card1.Rank)
            //|| (c3.Rank == card2.Rank)
            //|| (c4 != null && (
            //    (c4.Rank == card1.Rank)
            //    || (c4.Rank == card2.Rank)
            //    ))
            //|| (c5 != null && (
            //    (c5.Rank == card1.Rank)
            //    || (c5.Rank == card2.Rank)
            //    ))

            //(card1.Rank == card2.Rank && card1.Rank >= maxCard.Rank)
            //|| (c1.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
            //|| (c1.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
            //|| (c2.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
            //|| (c2.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
            //|| (c3.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
            //|| (c3.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
            //|| (c4 != null && (
            //    (c4.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
            //    || (c4.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
            //    ))
            //|| (c5 != null && (
            //    (c5.Rank == card1.Rank && card1.Rank >= maxCard.Rank)
            //    || (c5.Rank == card2.Rank && card2.Rank >= maxCard.Rank)
            //    ))
            return false;
        }

        private bool CheckFour(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            var lsMine = GetRankCount(new[] { card1, card2 });
            int cnt = 4;
            if (c4 == null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if(rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 != null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4, c5 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            return false;
        }

        private bool CheckOrCallPosaLimit(bool doFold)
        {

            if (_table.LimitByPosa)
                return CheckOrCall();
            else
            {
                if (!_table.ClickCheck())
                {
                    _table.Algo += "PosaLim";
                    LogApp("fold by limit", true);
                    if (doFold)
                        return _table.ClickFold();
                }
            }
            return false;
        }

        //private bool CheckOrCallBetLimit(bool doFold)
        //{

        //    if (_table.LimitByPosa)
        //        return CheckOrCall();
        //    else
        //    {
        //        if (!_table.ClickCheck())
        //        {
        //            _log.Info("fold by limit");
        //            if (doFold)
        //                return _table.ClickFold();
        //        }
        //    }
        //    return false;
        //}

        private bool CheckOrCallLimit(bool doFold)
        {
            //TODO Players
            //if (_table.PlayersRaisedCount > 1)
            //{
            //    if (!_table.ClickCheck())
            //    {
            //        _log.Info("fold too many raises");
            //        if (doFold)
            //            return _table.ClickFold();
            //    }
            //}

            //if (CurrentCounter <= GetRoundCallLimit)

            var limit = GetRoundCallLimit;

            //if (_table.IsTurn || _table.IsRiver)// на терне и ривере дорого много чекать
            //    limit = 1;

            if (CurrentCounter <= limit)
                return CheckOrCall();
            else
            {
                if (!_table.ClickCheck())
                {
                    LogApp("fold by limit", true);
                    if (doFold)
                        return _table.ClickFold();
                }
            }
            return false;
        }

        bool NeedBetOrRaise(bool validateLimit)
        {
            if (CurrentCounter <= GetRoundRaiseLimit)
            //if(_table.RaiseCounter == 0)
                return BetOrRaise(validateLimit);
            else
                return CheckOrCall();
        }

        int CurrentCounter
        {
            get
            {
                if (_table.IsFlop) return _table.BetCounterFlop;
                if (_table.IsTurn) return _table.BetCounterTurn;
                if (_table.IsRiver) return _table.BetCounterRiver;
                return _table.BetCounterPreFlop;
            }
        }

        int PrevCounter
        {
            get
            {
                if (_table.IsFlop) return _table.BetCounterPreFlop;
                if (_table.IsTurn) return _table.BetCounterFlop;
                if (_table.IsRiver) return _table.BetCounterTurn;
                throw new ArgumentOutOfRangeException("dont use it on preflop");
                return _table.BetCounterPreFlop;
            }
        }

        bool IsBet
        {
            get
            {
				return true;
                var rnd = new Random();
                var rndVal = rnd.Next(0, 100);
                return rndVal > 10;
            }
        }

        bool BetOrRaise(bool limitCheck)
        {
            if (!_table.ClickBet(limitCheck))
                if (!_table.ClickRaise(limitCheck))
                    if (!_table.ClickCall(limitCheck))
                        if (!_table.ClickCheck())
                        {
                            //Ex.Info("Can't bet or raise");
                            //return false;
                            return _table.ClickFold();
                        }

            return true;
        }
        bool CheckOrCall()
        {
            if (!_table.ClickCheck())
            {
                if (!_table.ClickCall(true))
                {
                    //Ex.Info("Can't check or call");
                    //return false;
                    return _table.ClickFold();
                }
            }
            return true;
        }

        //bool RaiseOrBetOrCallOrCheck()
        //{
        //    if (!_table.ClickRaise(true))
        //    {
        //        if (!_table.ClickBet(true))
        //        {
        //            if (!_table.ClickCall())
        //            {
        //                if (!_table.ClickCheck())
        //                {
        //                    //Ex.Info("Can't bet or call or check");
        //                    //return false;
        //                    return _table.ClickFold();
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        

        Dictionary<Rank, int> GetRankCount(IEnumerable<Card> input)
        {
            var ls = new List<Card>(input);

            var lsCnt = new Dictionary<Rank, int>();
            lsCnt.Add(Rank.Ace, ls.FindAll((card) => card.Rank == Rank.Ace).Count);
            lsCnt.Add(Rank.King, ls.FindAll((card) => card.Rank == Rank.King).Count);
            lsCnt.Add(Rank.Queen, ls.FindAll((card) => card.Rank == Rank.Queen).Count);
            lsCnt.Add(Rank.Jack, ls.FindAll((card) => card.Rank == Rank.Jack).Count);

            lsCnt.Add(Rank.Ten,    ls.FindAll((card) => card.Rank == Rank.Ten).Count);
            lsCnt.Add(Rank.Nine, ls.FindAll((card) => card.Rank == Rank.Nine).Count);
            lsCnt.Add(Rank.Eight, ls.FindAll((card) => card.Rank == Rank.Eight).Count);
            lsCnt.Add(Rank.Seven, ls.FindAll((card) => card.Rank == Rank.Seven).Count);
            lsCnt.Add(Rank.Six, ls.FindAll((card) => card.Rank == Rank.Six).Count);
            lsCnt.Add(Rank.Five, ls.FindAll((card) => card.Rank == Rank.Five).Count);
            lsCnt.Add(Rank.Four, ls.FindAll((card) => card.Rank == Rank.Four).Count);
            lsCnt.Add(Rank.Three, ls.FindAll((card) => card.Rank == Rank.Three).Count);
            lsCnt.Add(Rank.Two, ls.FindAll((card) => card.Rank == Rank.Two).Count);

            lsCnt.Add(Rank.NOT_RECOGNIZED, ls.FindAll((card) => card.Rank == Rank.NOT_RECOGNIZED).Count);
            return lsCnt;
        }

        Dictionary<Suit, int> GetSuitCount(IEnumerable<Card> input)
        {
            var ls = new List<Card>(input);

            var lsCnt = new Dictionary<Suit, int>();
            lsCnt.Add(Suit.Hearts, ls.FindAll((card) => card.Suit == Suit.Hearts).Count);
            lsCnt.Add(Suit.Diamonds, ls.FindAll((card) => card.Suit == Suit.Diamonds).Count);
            lsCnt.Add(Suit.Clubs, ls.FindAll((card) => card.Suit == Suit.Clubs).Count);
            lsCnt.Add(Suit.Spades, ls.FindAll((card) => card.Suit == Suit.Spades).Count);
            lsCnt.Add(Suit.NOT_RECOGNIZED, ls.FindAll((card) => card.Suit == Suit.NOT_RECOGNIZED).Count);
            
            return lsCnt;
        }

        bool CheckFlush(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            var lsMine = GetSuitCount(new[] { card1, card2 });
            int cnt = 5;
            if (c4 == null && c5 == null)
            {
                var lsAll = GetSuitCount(new[] { c1, c2, c3 });
                foreach (Suit rank in Enum.GetValues(typeof(Suit)))
                {
                    if (rank != Suit.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 == null)
            {
                var lsAll = GetSuitCount(new[] { c1, c2, c3, c4 });
                foreach (Suit rank in Enum.GetValues(typeof(Suit)))
                {
                    if (rank != Suit.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 != null)
            {
                var lsAll = GetSuitCount(new[] { c1, c2, c3, c4, c5 });
                foreach (Suit rank in Enum.GetValues(typeof(Suit)))
                {
                    if (rank != Suit.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            return false;
        }

        bool CheckHalfFlush(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            var lsMine = GetSuitCount(new[] { card1, card2 });
            int cnt = 4;
            if (c4 == null)
            {
                var lsAll = GetSuitCount(new[] { c1, c2, c3 });
                foreach (Suit rank in Enum.GetValues(typeof(Suit)))
                {
                    if (rank != Suit.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null)
            {
                var lsAll = GetSuitCount(new[] { c1, c2, c3, c4 });
                foreach (Suit rank in Enum.GetValues(typeof(Suit)))
                {
                    if (rank != Suit.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            return false;
        }

        bool CheckTriple(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            var lsMine = GetRankCount(new[] { card1, card2 });
            int cnt = 3;
            if (c4 == null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            if (c4 != null && c5 != null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4, c5 });
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank != Rank.NOT_RECOGNIZED)
                        if (lsAll[rank] + lsMine[rank] >= cnt && lsMine[rank] > 0)
                        {
                            return true;
                        }
                }
            }
            return false;
        }
       
        bool CheckTwoPair(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3 });
                if (lsAll[card1.Rank] >= 1 && lsAll[card2.Rank] >= 1)
                    return true;
                
            }
            if (c4 != null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4 });
                if (lsAll[card1.Rank] >= 1 && lsAll[card2.Rank] >= 1)
                    return true;
            }
            if (c4 != null && c5 != null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4, c5 });
                if (lsAll[card1.Rank] >= 1 && lsAll[card2.Rank] >= 1)
                    return true;
            }
            return false;
        }

        bool CheckPair(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2, Rank maxRank)
        {
            if (card1.Rank == card2.Rank && card1.Rank >= maxRank)
                return true;

            if (c4 == null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3 });
                if (
                    lsAll[card1.Rank] >= 1 && card1.Rank >= maxRank
                    ||
                    lsAll[card2.Rank] >= 1 && card2.Rank >= maxRank
                    )
                    return true;

            }
            if (c4 != null && c5 == null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4 });
                if (
                    lsAll[card1.Rank] >= 1 && card1.Rank >= maxRank
                    ||
                    lsAll[card2.Rank] >= 1 && card2.Rank >= maxRank
                    )
                    return true;
            }
            if (c4 != null && c5 != null)
            {
                var lsAll = GetRankCount(new[] { c1, c2, c3, c4, c5 });
                if (
                    lsAll[card1.Rank] >= 1 && card1.Rank >= maxRank
                    ||
                    lsAll[card2.Rank] >= 1 && card2.Rank >= maxRank
                    )
                    return true;
            }
            return false;
        }


        

        bool CheckStreet(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                return CheckStreetAlgo(new[] {c1, c2, c3, card1, card2 }, 5);
            }
            if (c4 != null && c5 == null)
            {
                return CheckStreetAlgo(new[] { c1, c2, c3, c4, card1, card2 }, 5);
            }
            if (c4 != null && c5 != null)
            {
                return 
                    CheckStreetAlgo(new[] { c1, c2, c3, c5, card1, card2 }, 5)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c3, c4, card1, card2 }, 5)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c3, c5, card1, card2 }, 5)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c4, c5, card1, card2 }, 5)
                    ||
                    CheckStreetAlgo(new[] { c2, c3, c4, c5, card1, card2 }, 5);
            }
            return false;
        }

        bool CheckOesdStreet(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            if (c4 == null)
            {
                return CheckStreetAlgo(new[] { c1, c2, c3, card1, card2 }, 4);
            }
            if (c4 != null)
            {
                return
                    CheckStreetAlgo(new[] { c1, c2, c3, card1, card2 }, 4)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c4, card1, card2 }, 4)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c3, c4, card1 }, 4)
                    ||
                    CheckStreetAlgo(new[] { c1, c2, c3, c4, card2 }, 4)
                    ;
            }
            return false;
        }

        /// <summary>
        /// гатшот
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <param name="c4"></param>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <returns></returns>
        bool CheckHalfStreet(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            if (c4 == null)
            {
                return CheckHalfStreetAlgo(new[] { c1, c2, c3, card1, card2 }, 4);
            }
            if (c4 != null)
            {
                return 
                    CheckHalfStreetAlgo(new[] { c1, c2, c3, card1, card2 }, 4)
                    ||
                    CheckHalfStreetAlgo(new[] { c1, c2, c4, card1, card2 }, 4)
                    ||
                    CheckHalfStreetAlgo(new[] { c1, c2, c3, c4, card1 }, 4)
                    ||
                    CheckHalfStreetAlgo(new[] { c1, c2, c3, c4, card2 }, 4)
                    ;
            }
            return false;
        }

        bool CheckHalfStreetAlgo(Card[] data, int halfStreetLength)
        {
            var ls = new List<Card>(data);
            ls.Sort((x, y) => x.Rank.CompareTo(y.Rank));

            var ids = new List<int>();
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Two).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Three).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Four).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Five).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Six).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Seven).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Eight).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Nine).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Ten).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Jack).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Queen).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.King).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i)=> i.Rank == Rank.Ace).Count > 0 ? 1 : 0);

            bool d = false;
            int lineCnt = 0;
            int zeroCnt = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                if (d && (ids[i] == 1))
                    lineCnt++;

                if (lineCnt + 1 >= halfStreetLength)
                    return true;

                if (d && (ids[i] == 0))
                    zeroCnt++;


                if (ids[i] == 1)
                    d = true;
                else
                {
                    if (zeroCnt > 1)
                    {
                        d = false;
                        zeroCnt = 0;
                        lineCnt = 0;
                    }
                }
            }
            
            return false;
        }

        bool CheckStreetAlgo(Card[] data, int streetLength)
        {
            var ls = new List<Card>(data);
            ls.Sort((x, y) => x.Rank.CompareTo(y.Rank));

            var ids = new List<int>();
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Two).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Three).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Four).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Five).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Six).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Seven).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Eight).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Nine).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Ten).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Jack).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Queen).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.King).Count > 0 ? 1 : 0);
            ids.Add(ls.FindAll((i) => i.Rank == Rank.Ace).Count > 0 ? 1 : 0);

            bool d = false;
            int lineCnt = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                if (d && (ids[i] == 1))
                    lineCnt++;

                if (lineCnt + 1 >= streetLength)
                    return true;

                if (ids[i] == 1)
                    d = true;
                else
                {
                    d = false;
                    lineCnt = 0;
                }

            }
            
            return false;
        }
        
        
        private void cbTopMost_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = cbTopMost.Checked;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var s = new SettingsForm();
            s.ShowDialog();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (rbSix.Checked)
            {
                _settings = AppSettingsManager.Load("six");
                Init(_settings);

            }
            if (rbNine.Checked)
            {
                _settings = AppSettingsManager.Load("nine");
                Init(_settings);
            }
        }

        private AppSettings _settings;
        private void cbTableType_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            
        }

        private void InitPlayerRects(AppSettings _settings)
        {
            if (_table.Players.Count == 6)
            {
                _table.PlayerRects.Clear();
                _table.PlayerRects.AddRange(new[]
                {
                    _settings.Player1, _settings.Player2, _settings.Player3,
                    _settings.Player4, _settings.Player5, _settings.Player6
                });

                _table.PlayerFolds.Clear();
                _table.PlayerFolds.AddRange(new[]
                {
                    _settings.Fold1, _settings.Fold2, _settings.Fold3,
                    _settings.Fold4, _settings.Fold5, _settings.Fold6
                });
            }
            if (_table.Players.Count == 9)
            {
                _table.PlayerRects.Clear();
                _table.PlayerRects.AddRange(new[]
                {
                    _settings.Player1, _settings.Player2, _settings.Player3,
                    _settings.Player4, _settings.Player5, _settings.Player6, 
                    _settings.Player7, _settings.Player8,
                    _settings.Player9
                });

                //TODO 9 players
                _table.PlayerFolds.Clear();
                _table.PlayerFolds.AddRange(new[]
                {
                    _settings.Fold1, _settings.Fold2, _settings.Fold3,
                    _settings.Fold4, _settings.Fold5, _settings.Fold6,
                    _settings.Fold7, _settings.Fold8, _settings.Fold9
                });
            }
        }

        private void InitButtonRects(AppSettings _settings)
        {
            if (_table.DealerButtons.Count == 6)
            {
                _table.ButtonRects.Clear();
                _table.ButtonRects.AddRange(new[]
                {
                    _settings.Button1, _settings.Button2, _settings.Button3,
                    _settings.Button4, _settings.Button5, _settings.Button6
                });
            }
            if (_table.DealerButtons.Count == 9)
            {
                _table.ButtonRects.Clear();
                _table.ButtonRects.AddRange(new[]
                {
                    _settings.Button1, _settings.Button2, _settings.Button3,
                    _settings.Button4, _settings.Button5, _settings.Button6, 
                    _settings.Button7, _settings.Button8,
                    _settings.Button9
                });
            }
        }

        private void InitBetRects(AppSettings _settings)
        {
            if (_table.PlayerBets.Count == 6)
            {
                _table.BetRects.Clear();
                _table.BetRects.AddRange(new[]
                {
                    _settings.Bet1, _settings.Bet2, _settings.Bet3,
                    _settings.Bet4, _settings.Bet5, _settings.Bet6
                });
            }
            //TODO Bet fo 10 players
            //if (_table.DealerButtons.Count == 11)
            //{
            //    _table.ButtonRects.Clear();
            //    _table.ButtonRects.AddRange(new[]
            //    {
            //        _settings.Button1, _settings.Button2, _settings.Button3,
            //        _settings.Button4, _settings.Button5, _settings.Button6, 
            //        _settings.Button7, _settings.Button8,
            //        _settings.Button9
            //    });
            //}
        }

        private void Init(AppSettings settings)
        {

            if (settings.TimerInterval == 0)
                timer1.Interval = 1000;
            else
                timer1.Interval = settings.TimerInterval;

            tbDirTestFiles.Text = settings.TestFolder;

            if (File.Exists(settings.TemplatePlayersFile))
            {
                _playersTemplate = Bitmap.FromFile(settings.TemplatePlayersFile) as Bitmap;
            }
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            if (Width > 500)
            {
                Width = 500;
            }
            else
            {
                Width = 1000;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void lblPlayersCount_Click(object sender, EventArgs e)
        {

        }

        private void lblAlgoResult_Click(object sender, EventArgs e)
        {

        }

        private void lblBetCounter_Click(object sender, EventArgs e)
        {

        }

        private void lblTimerInterval_Click(object sender, EventArgs e)
        {

        }

        private void btnPatterns_Click(object sender, EventArgs e)
        {
            var s = new PatternsForm();
            s.ShowDialog();
        }

        private void tmMouseTremor_Tick(object sender, EventArgs e)
        {

        }

        private void cbAlgo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btGameTest_Click(object sender, EventArgs e)
        {
            _table.ClickPause();
        }

        private void cbSaveTableBmp_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            LoadSettings();
            _table = new Table((rbSix.Checked ? 6 : 9), _settings);
            _table.ClickLog += TableOnClickLog;
            _table.ClickLogDebug += TableOnClickLogDebug;
            _table.AfterClickLog += TableOnAfterClickLog;
            InitPlayerRects(_settings);
            InitButtonRects(_settings);
            InitBetRects(_settings);
        }

        void TableOnClickLogDebug(string msg)
        {
            return;
            tbLog.Text += msg + Environment.NewLine;
        }

        private void cbShowLog_CheckedChanged(object sender, EventArgs e)
        {
            tbLog.Visible = cbShowLog.Checked;
        }

        public string Perform(ActionType actionType, params object[] args)
        {
            var result = "";
            try
            {
                switch (actionType)
                {
                    case ActionType.None:
                        break;
                    case ActionType.r:
                        SyncContext.Send(state =>
                        {
                            if (_table != null)
                                result = "rebuy cnt="+ _rebuyCount + ";" + _lastRebuy;
                        }, null);
                        break;
                    case ActionType.s:
                        SyncContext.Send(state =>
                            {
                                if (_table != null)
                                    result = _table.MyBalance.Posa.ToString() 
                                        + ";" + _table.Bank.TotalValue 
                                        + ";" + (_table.MyCards.Count > 0 ? "in-game" : "off-game") 
                                        + ";" + _lastCloseReason;
                            }, null);
                        break;
                    case ActionType.Log:
                        break;
                    case ActionType.list:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("actionType");
                }

                
            }
            catch (Exception exception)
            {
                result = exception.ToString();
            }
            return result;
        }

        public SynchronizationContext SyncContext { get; private set; }

        private void cbJabberBot_CheckedChanged(object sender, EventArgs e)
        {
            ProcessJabberBot();
        }

        private void ProcessJabberBot()
        {
            if (cbJabberBot.Checked)
                MyBot.Start();
            //new Thread(() => MyBot.Start()).Start();
            else
                MyBot.Stop();
        }

        private void btnArchive_Click(object sender, EventArgs e)
        {
            //TODO
        }
    }
}
