using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using log4net;
using miranda.ui;
using AForge.Imaging.Filters;
using AForge.Imaging;
using System.Drawing.Imaging;

namespace miranda
{
    public partial class SettingsForm : Form
    {
        readonly KeyboardHook _hook = new KeyboardHook();
        private readonly ILog _log;

        public SettingsForm()
        {
            InitializeComponent();
            
            _hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(_hook_KeyPressed);
            _hook.RegisterHotKey(miranda.ModifierKeys.Shift, Keys.Escape);


            _log = LogManager.GetLogger("Application");
            
            
            //tbFile.Text = Settings.Default.TemplateFile;
            
            //lblAllCardsRect.Text = Settings.Default.AllCardsRect.ToString();

            //lblTableArea.Text = Settings.Default.TableRect.ToString();

            //lblMyCardsRect.Text = Settings.Default.MyCardsRect.ToString();

            //lblButtonsRect.Text = Settings.Default.ButtonsRect.ToString();

            //lblTableWindow.Text = "WindowPoint = " + Settings.Default.WindowPoint;

            //tbTimer.Text = Settings.Default.TimerInterval.ToString();

            //if (File.Exists(Settings.Default.TemplatePlayersFile))
            //{
            //    tbFilePlayers.Text = Settings.Default.TemplatePlayersFile;
            //}
            
            


            //tbMinBet.Value = Settings.Default.MinBet;

            //tbWaitRoundLimit.Value = Settings.Default.WaitRoundLimit;
        }

        private void _hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == miranda.ModifierKeys.Shift)
            {
                var handle = WinApi.WindowFromPoint(Cursor.Position);
                WinApi.RECT rect;
                WinApi.GetWindowRect(handle, out rect);

                lblTableWindow.Text = "Handle " + handle.ToString() + "; WindowPoint = " + rect.Left + ", " + rect.Top;

                _settings.WindowPoint = new Point((int)rect.Left, (int)rect.Top);
                AppSettingsManager.Save(_settings);

                DrawRect(rect);
            }
        }

        private void DrawRect(WinApi.RECT rect)
        {
            IntPtr desktopPtr = WinApi.GetDC(IntPtr.Zero);
            Graphics g = Graphics.FromHdc(desktopPtr);

            var b = new SolidBrush(Color.White);

            var r = new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);

            //g.FillRectangle(b, r);

            var pen = new Pen(Color.Black);
            pen.Width = 3;
            g.DrawLine(pen, r.Left, r.Bottom, r.Right, r.Bottom);
            g.DrawLine(pen, r.Left, r.Top, r.Left, r.Bottom);

            g.DrawLine(pen, r.Left, r.Top, r.Right, r.Top);
            g.DrawLine(pen, r.Right, r.Top, r.Right, r.Bottom);


            g.Dispose();
            WinApi.ReleaseDC(IntPtr.Zero, desktopPtr);
        }

        private void btSelectDir_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFile.Text = openFileDialog1.FileName;
                _settings.TemplateFile = tbFile.Text;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnSelectRect_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.AllCardsRect);
            f.Text = "Select All Cards rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblAllCardsRect.Text = f.Rect.ToString();
                _settings.AllCardsRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnSelectRectMyCards_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.MyCardsRect);
            f.Text = "Select My Cards rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblMyCardsRect.Text = f.Rect.ToString();
                _settings.MyCardsRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        

        private void btnSelectTable_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.TableRect);
            f.Text = "Select table rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblTableArea.Text = f.Rect.ToString();
                _settings.TableRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnSelectTemplatePlayers_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFilePlayers.Text = openFileDialog1.FileName;
                _settings.TemplatePlayersFile = tbFilePlayers.Text;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbMinBet_ValueChanged(object sender, EventArgs e)
        {
            _settings.MinBet = tbMinBet.Value;
            AppSettingsManager.Save(_settings);
        }

        private void tbWaitRoundLimit_ValueChanged(object sender, EventArgs e)
        {
            _settings.WaitRoundLimit = tbWaitRoundLimit.Value;
            AppSettingsManager.Save(_settings);
        }

        private void tbTimer_TextChanged(object sender, EventArgs e)
        {
            
        }

        public AppSettings CurrentSettings { get { return _settings; } }

        private AppSettings _settings;
        private void cbTableType_SelectedIndexChanged(object sender, EventArgs e)
        {
           

        }

        private void InitUI(AppSettings settings)
        {
            tbFile.Text = settings.TemplateFile;

            tbBluff.Value = settings.BluffPercent;
            tbShortStackPercent.Value = settings.ShortStackPercent;

            lblAllCardsRect.Text = settings.AllCardsRect.ToString();

            lblTableArea.Text = settings.TableRect.ToString();

            lblMyCardsRect.Text = settings.MyCardsRect.ToString();

            lblTableWindow.Text = "WindowPoint = " + settings.WindowPoint;

            tbTimer.Text = settings.TimerInterval.ToString();

            if (File.Exists(settings.TemplatePlayersFile))
            {
                tbFilePlayers.Text = settings.TemplatePlayersFile;
            }

            tbMinBet.Value = settings.MinBet;

            tbWaitRoundLimit.Value = settings.WaitRoundLimit;
			
			tbPreFlopWaitRoundLimit.Value = settings.PreFlopWaitRoundLimit;

            lblPlayer1.Text = settings.Player1.ToString();
            lblPlayer2.Text = settings.Player2.ToString();
            lblPlayer3.Text = settings.Player3.ToString();
            lblPlayer4.Text = settings.Player4.ToString();
            lblPlayer5.Text = settings.Player5.ToString();
            lblPlayer6.Text = settings.Player6.ToString();
            lblPlayer7.Text = settings.Player7.ToString();
            lblPlayer8.Text = settings.Player8.ToString();
            lblPlayer9.Text = settings.Player9.ToString();

            lblButton1.Text = settings.Button1.ToString();
            lblButton2.Text = settings.Button2.ToString();
            lblButton3.Text = settings.Button3.ToString();
            lblButton4.Text = settings.Button4.ToString();
            lblButton5.Text = settings.Button5.ToString();
            lblButton6.Text = settings.Button6.ToString();
            lblButton7.Text = settings.Button7.ToString();
            lblButton8.Text = settings.Button8.ToString();
            lblButton9.Text = settings.Button9.ToString();

            lblShort1.Text = settings.ShortButtonRect1.ToString();
            lblShort2.Text = settings.ShortButtonRect2.ToString();
            lblShort3.Text = settings.ShortButtonRect3.ToString();
            lblShort4.Text = settings.ShortButtonRect4.ToString();

            lblMyBalance.Text = settings.BalanceRect.ToString();

            lblBankSize.Text = settings.BankSizeRect.ToString();
            lblCallRate.Text = settings.CallRateRect.ToString();

            cbRemoveDollarSign.CheckedChanged -= cbRemoveDollarSign_CheckedChanged;
            cbRemoveDollarSign.Checked = settings.RemoveDollar;
            cbRemoveDollarSign.CheckedChanged += cbRemoveDollarSign_CheckedChanged;

            lblBetInput.Text = settings.BetInputRect.ToString();

            lblFold.Text = settings.FoldRect.ToString();
            lblCheckCall.Text = settings.CheckCallRect.ToString();
            lblBetRaise.Text = settings.BetRaiseRect.ToString();

            lblBet1.Text = settings.Bet1.ToString();
            lblBet2.Text = settings.Bet2.ToString();
            lblBet3.Text = settings.Bet3.ToString();
            lblBet4.Text = settings.Bet4.ToString();
            lblBet5.Text = settings.Bet5.ToString();
            lblBet6.Text = settings.Bet6.ToString();

            nbBuyIn.Value = settings.BuyIn;
            tbTotalLimit.Value = settings.Limit;
            tbRebuyLevel.Value = settings.RebuyLevel;
            lblRaiseRate.Text = settings.RaiseRateRect.ToString();

            lblFold1.Text = settings.Fold1.ToString();
            lblFold2.Text = settings.Fold2.ToString();
            lblFold3.Text = settings.Fold3.ToString();
            lblFold4.Text = settings.Fold4.ToString();
            lblFold5.Text = settings.Fold5.ToString();
            lblFold6.Text = settings.Fold6.ToString();
            lblFold7.Text = settings.Fold7.ToString();
            lblFold8.Text = settings.Fold8.ToString();
            lblFold9.Text = settings.Fold9.ToString();

            lblResume.Text = settings.ResumeRect.ToString();
            lblPause.Text = settings.PauseRect.ToString();

            lblRectBuyInOk.Text = settings.RectBuyInOk.ToString();
            lblRectCancelTable.Text = settings.RectCancelTable.ToString();
            lblRectLeaveTableOk.Text = settings.RectLeaveTableOk.ToString();
            lblRectOpenTable.Text = settings.RectOpenTable.ToString();
            lblRectTableX.Text = settings.RectTableX.ToString();

            lblCardSuit.Text = settings.CardSuitRect.ToString();
            lblCardRank.Text = settings.CardRankRect.ToString();
            tbMinCardSize.Value = settings.MinCardSize;

            lblTxtResult.Text = settings.TxtResultRect.ToString();

            lblTotalSize.Text = settings.TotalSizeRect.ToString();
        }

        private void btnPlayer1_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player1);
            f.Text = "Select Player1 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer1.Text = f.Rect.ToString();
                _settings.Player1 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbPreFlopWaitRoundLimit_ValueChanged(object sender, EventArgs e)
        {
            _settings.PreFlopWaitRoundLimit = tbPreFlopWaitRoundLimit.Value;
            AppSettingsManager.Save(_settings);
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _hook.Dispose();
        }

        private void btnPlayer2_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player2);
            f.Text = "Select Player2 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer2.Text = f.Rect.ToString();
                _settings.Player2 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer3_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player3);
            f.Text = "Select Player3 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer3.Text = f.Rect.ToString();
                _settings.Player3 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer4_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player4);
            f.Text = "Select Player4 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer4.Text = f.Rect.ToString();
                _settings.Player4 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer5_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player5);
            f.Text = "Select Player5 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer5.Text = f.Rect.ToString();
                _settings.Player5 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer6_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player6);
            f.Text = "Select Player6 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer6.Text = f.Rect.ToString();
                _settings.Player6 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer7_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player7);
            f.Text = "Select Player7 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer7.Text = f.Rect.ToString();
                _settings.Player7 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer8_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player8);
            f.Text = "Select Player8 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer8.Text = f.Rect.ToString();
                _settings.Player8 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnPlayer9_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Player9);
            f.Text = "Select Player9 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPlayer9.Text = f.Rect.ToString();
                _settings.Player9 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnMyBalance_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.BalanceRect);
            f.Text = "Select Balance rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblMyBalance.Text = f.Rect.ToString();
                _settings.BalanceRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBankSize_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.BankSizeRect);
            f.Text = "Select BankSize rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBankSize.Text = f.Rect.ToString();
                _settings.BankSizeRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnCallRate_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.CallRateRect);
            f.Text = "Select CallRate rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblCallRate.Text = f.Rect.ToString();
                _settings.CallRateRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton1_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button1);
            f.Text = "Select Button1 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton1.Text = f.Rect.ToString();
                _settings.Button1 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton2_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button2);
            f.Text = "Select Button2 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton2.Text = f.Rect.ToString();
                _settings.Button2 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton3_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button3);
            f.Text = "Select Button3 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton3.Text = f.Rect.ToString();
                _settings.Button3 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton4_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button4);
            f.Text = "Select Button4 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton4.Text = f.Rect.ToString();
                _settings.Button4 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton5_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button5);
            f.Text = "Select Button5 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton5.Text = f.Rect.ToString();
                _settings.Button5 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton6_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button6);
            f.Text = "Select Button6 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton6.Text = f.Rect.ToString();
                _settings.Button6 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton7_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button7);
            f.Text = "Select Button7 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton7.Text = f.Rect.ToString();
                _settings.Button7 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton8_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button8);
            f.Text = "Select Button8 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton8.Text = f.Rect.ToString();
                _settings.Button8 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnButton9_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Button9);
            f.Text = "Select Button9 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblButton9.Text = f.Rect.ToString();
                _settings.Button9 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnGetDefaultSettings_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show("Overwrite settings with default values?", "Info", MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button3) == DialogResult.Yes)
            {
                var txt = "";

                if (rbSix.Checked)
                    txt = "six";
                if (rbNine.Checked)
                    txt = "nine";
                var file = Application.StartupPath + @"\default-settings\" + txt +
                           ".xml";
                if (File.Exists(file))
                {
                    File.Copy(file, Application.StartupPath + @"\" + txt +".xml", true);
                }
            }
        }

        private void btnShort1_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.ShortButtonRect1);
            f.Text = "Select Short button 1 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblShort1.Text = f.Rect.ToString();
                _settings.ShortButtonRect1 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnShort2_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.ShortButtonRect2);
            f.Text = "Select Short button 2 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblShort2.Text = f.Rect.ToString();
                _settings.ShortButtonRect2 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnShort3_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.ShortButtonRect3);
            f.Text = "Select Short button 3 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblShort3.Text = f.Rect.ToString();
                _settings.ShortButtonRect3 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnShort4_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.ShortButtonRect4);
            f.Text = "Select Short button 4 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblShort4.Text = f.Rect.ToString();
                _settings.ShortButtonRect4 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void cbRemoveDollarSign_CheckedChanged(object sender, EventArgs e)
        {
            _settings.RemoveDollar = cbRemoveDollarSign.Checked;
            AppSettingsManager.Save(_settings);
        }

        private void btnBetInput_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.BetInputRect);
            f.Text = "Select Bet input rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBetInput.Text = f.Rect.ToString();
                _settings.BetInputRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void nbBuyIn_ValueChanged(object sender, EventArgs e)
        {
            _settings.BuyIn = nbBuyIn.Value;
            AppSettingsManager.Save(_settings);
        }

        private void btnFold_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.FoldRect);
            f.Text = "Select Fold button rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold.Text = f.Rect.ToString();
                _settings.FoldRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnCheckCall_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.CheckCallRect);
            f.Text = "Select Check/Call button rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblCheckCall.Text = f.Rect.ToString();
                _settings.CheckCallRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBetRaise_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.BetRaiseRect);
            f.Text = "Select Check/Call button rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBetRaise.Text = f.Rect.ToString();
                _settings.BetRaiseRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet1_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet1);
            f.Text = "Select Bet1 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet1.Text = f.Rect.ToString();
                _settings.Bet1 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet2_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet2);
            f.Text = "Select Bet2 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet2.Text = f.Rect.ToString();
                _settings.Bet2 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet3_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet3);
            f.Text = "Select Bet3 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet3.Text = f.Rect.ToString();
                _settings.Bet3 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet4_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet4);
            f.Text = "Select Bet4 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet4.Text = f.Rect.ToString();
                _settings.Bet4 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet5_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet5);
            f.Text = "Select Bet5 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet5.Text = f.Rect.ToString();
                _settings.Bet5 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnBet6_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Bet6);
            f.Text = "Select Bet6 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblBet6.Text = f.Rect.ToString();
                _settings.Bet6 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnRaiseRate_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RaiseRateRect);
            f.Text = "Select RaiseRate rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRaiseRate.Text = f.Rect.ToString();
                _settings.RaiseRateRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold1_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold1);
            f.Text = "Select Fold1 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold1.Text = f.Rect.ToString();
                _settings.Fold1 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold2_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold2);
            f.Text = "Select Fold2 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold2.Text = f.Rect.ToString();
                _settings.Fold2 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold3_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold3);
            f.Text = "Select Fold3 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold3.Text = f.Rect.ToString();
                _settings.Fold3 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold4_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold4);
            f.Text = "Select Fold4 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold4.Text = f.Rect.ToString();
                _settings.Fold4 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold5_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold5);
            f.Text = "Select Fold5 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold5.Text = f.Rect.ToString();
                _settings.Fold5 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold6_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold6);
            f.Text = "Select Fold6 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold6.Text = f.Rect.ToString();
                _settings.Fold6 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbTimer_ValueChanged(object sender, EventArgs e)
        {
            
            
        }

        private void tbTimer_TextChanged_1(object sender, EventArgs e)
        {
            int t;
            if (int.TryParse(tbTimer.Text, out t))
            {
                _settings.TimerInterval = t;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnRandomTableRect_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RandomTableRect);
            f.Text = "Select RandomTableRect rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRandomTableRect.Text = f.Rect.ToString();
                _settings.RandomTableRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.PauseRect);
            f.Text = "Select Pause rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblPause.Text = f.Rect.ToString();
                _settings.PauseRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btResume_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.ResumeRect);
            f.Text = "Select Resume rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblResume.Text = f.Rect.ToString();
                _settings.ResumeRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold7_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold7);
            f.Text = "Select Fold7 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold7.Text = f.Rect.ToString();
                _settings.Fold7 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold8_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold8);
            f.Text = "Select Fold8 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold8.Text = f.Rect.ToString();
                _settings.Fold8 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnFold9_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.Fold9);
            f.Text = "Select Fold9 rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblFold9.Text = f.Rect.ToString();
                _settings.Fold9 = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            if (rbSix.Checked)
            {
                _settings = AppSettingsManager.Load("six");
                InitUI(_settings);

            }
            if (rbNine.Checked)
            {
                _settings = AppSettingsManager.Load("nine");
                InitUI(_settings);
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            AppSettingsManager.Save(_settings);
        }

        private void tbTotalLimit_ValueChanged(object sender, EventArgs e)
        {
            _settings.Limit = tbTotalLimit.Value;
            AppSettingsManager.Save(_settings);
        }

        private void btRectOpenTable_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RectOpenTable);
            f.Text = "Select RectOpenTable rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRectOpenTable.Text = f.Rect.ToString();
                _settings.RectOpenTable = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btRectCancelTable_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RectCancelTable);
            f.Text = "Select RectCancelTable rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRectCancelTable.Text = f.Rect.ToString();
                _settings.RectCancelTable = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btRectBuyInOk_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RectBuyInOk);
            f.Text = "Select RectBuyInOk rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRectBuyInOk.Text = f.Rect.ToString();
                _settings.RectBuyInOk = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btRectTableX_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RectTableX);
            f.Text = "Select RectTableX rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRectTableX.Text = f.Rect.ToString();
                _settings.RectTableX = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btRectLeaveTableOk_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.RectLeaveTableOk);
            f.Text = "Select RectLeaveTableOk rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblRectLeaveTableOk.Text = f.Rect.ToString();
                _settings.RectLeaveTableOk = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbRebuyLevel_ValueChanged(object sender, EventArgs e)
        {
            _settings.RebuyLevel = tbRebuyLevel.Value;
            AppSettingsManager.Save(_settings);
        }

        private void btnCardSuit_Click(object sender, EventArgs e)
        {
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            Crop crop = new Crop(_settings.MyCardsRect);//TODO card identity
            var source = crop.Apply(image);

            FiltersSequence seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);  //First add  grayScaling filter
            seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            var temp = seq.Apply(source); // Apply filters on source image
            
            
            BlobCounter extractor = new BlobCounter();
            extractor.FilterBlobs = true;
            extractor.MinWidth = extractor.MinHeight = (int)_settings.MinCardSize;//TODO card size
            //extractor.MaxWidth = extractor.MaxHeight = 70;//TODO card size
            extractor.ProcessImage(temp);

            //Will be used transform(extract) cards on source image 
            //QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();


            Bitmap cardImg = null;
            foreach (Blob blob in extractor.GetObjectsInformation())
            {
                cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);
                break;
            }

            var r = _settings.CardSuitRect;
            var f = new AreaSelectorForm(new Rectangle(r.X*5, r.Y*5, r.Width*5, r.Height*5));
            f.Text = "Select CardSuitRect rect";
            
            f.SetImage(cardImg, 5);
            if (f.ShowDialog() == DialogResult.OK)
            {
                var newRect = new Rectangle(
                    (int)Math.Round(f.Rect.X / 5.0),
                    (int)Math.Round(f.Rect.Y / 5.0),
                    (int)Math.Round(f.Rect.Width / 5.0),
                    (int)Math.Round(f.Rect.Height / 5.0));
                lblCardSuit.Text = newRect.ToString();
                _settings.CardSuitRect = newRect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnCardRank_Click(object sender, EventArgs e)
        {
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            Crop crop = new Crop(_settings.MyCardsRect);//TODO card identity
            var source = crop.Apply(image);

            FiltersSequence seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);  //First add  grayScaling filter
            seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            var temp = seq.Apply(source); // Apply filters on source image


            BlobCounter extractor = new BlobCounter();
            extractor.FilterBlobs = true;
            extractor.MinWidth = extractor.MinHeight = (int)_settings.MinCardSize;//TODO card size
            //extractor.MaxWidth = extractor.MaxHeight = 70;//TODO card size
            extractor.ProcessImage(temp);

            //Will be used transform(extract) cards on source image 
            //QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();


            Bitmap cardImg = null;
            foreach (Blob blob in extractor.GetObjectsInformation())
            {
                cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);
                break;
            }

            var r = _settings.CardRankRect;
            var f = new AreaSelectorForm(new Rectangle(r.X * 5, r.Y * 5, r.Width * 5, r.Height * 5));
            f.Text = "Select CardRankRect rect";

            f.SetImage(cardImg, 5);
            if (f.ShowDialog() == DialogResult.OK)
            {
                var newRect = new Rectangle(
                    (int)Math.Round(f.Rect.X / 5.0),
                    (int)Math.Round(f.Rect.Y / 5.0),
                    (int)Math.Round(f.Rect.Width / 5.0),
                    (int)Math.Round(f.Rect.Height / 5.0));
                lblCardRank.Text = newRect.ToString();
                _settings.CardRankRect = newRect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbMinCardSize_ValueChanged(object sender, EventArgs e)
        {
            _settings.MinCardSize = tbMinCardSize.Value;
            AppSettingsManager.Save(_settings);
        }

        private void btnTxtResult_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.TxtResultRect);
            f.Text = "Select TxtResultRect rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblTxtResult.Text = f.Rect.ToString();
                _settings.TxtResultRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnTotalSize_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(_settings.TotalSizeRect);
            f.Text = "Select TotalSize rect";
            var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
            f.SetImage(image);
            if (f.ShowDialog() == DialogResult.OK)
            {
                lblTotalSize.Text = f.Rect.ToString();
                _settings.TotalSizeRect = f.Rect;
                AppSettingsManager.Save(_settings);
            }
        }

        private void tbBluff_ValueChanged(object sender, EventArgs e)
        {
            _settings.BluffPercent = tbBluff.Value;
            AppSettingsManager.Save(_settings);
        }

        private void tbShortStackPercent_ValueChanged(object sender, EventArgs e)
        {
            _settings.ShortStackPercent = tbShortStackPercent.Value;
            AppSettingsManager.Save(_settings);
        }

       

        
    }
}
