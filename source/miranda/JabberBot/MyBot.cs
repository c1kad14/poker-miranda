using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using agsXMPP;
using agsXMPP.net;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.ping;
using agsXMPP.protocol.iq.time;
using agsXMPP.protocol.iq.version;
using log4net;
using miranda.Properties;
using File = System.IO.File;
using Message = agsXMPP.protocol.client.Message;
using Ping = agsXMPP.protocol.extensions.ping.Ping;
using Timer = System.Threading.Timer;

namespace miranda.JabberBot
{
    public static class MyBot
    {
        private static XmppClientConnection _mCon;

         static string ConnectServer;
         static string Server;
         static string UserName;
         static string Password;
        private static Timer _checkTimer;

        public static bool ResponceToWeb = false;
        

         static void Disconnect()
        {
            if (_mCon != null)
            {
                _mCon.SocketDisconnect();
                _mCon.Close();
                _mCon = null;
            }
        }

         static int _timerInterval = 5;

        private static bool Ping(string address)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            PingReply reply = null;
            try
            {
                reply = ping.Send(address);
            }
            catch (Exception e)
            {
                return false;
            }

            if (reply != null)
                return reply.Status == IPStatus.Success;

            return false;
        }

        static void TimerCallback(object state)
        {
            try
            {
                _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //if (!Ping("ya.ru"))
                {
                    //Log("Restarting jabberBot");
                    Disconnect();
                    Connect();
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("Debug").Error(e.Message);
            }
            finally
            {
                _checkTimer.Change(TimeSpan.FromMinutes(_timerInterval), TimeSpan.FromMinutes(_timerInterval));
            }
        }
        public static void Start()
        {
            Connect();
            if (_checkTimer == null)
            {
                _checkTimer = new Timer(TimerCallback);
            }
            _checkTimer.Change(TimeSpan.FromMinutes(_timerInterval), TimeSpan.FromMinutes(_timerInterval));
        }

        public static void Stop()
        {
            if (_checkTimer != null)
            {
                _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);

            }
            Disconnect();
        }

        

 

 

        static void Connect()
        {
            try
            {

                //var test = new MyBotTest();
                //test.ConnectTest(Settings.Default.ConnectServer, 5223);

                if(_mCon != null)
                {
                    Disconnect();
                }
                _mCon = new XmppClientConnection();


                _mCon.Resource = null;
                _mCon.Priority = 100;
                _mCon.Port = 5223;
                _mCon.ConnectServer = Settings.Default.ConnectServer;
                _mCon.Server = Settings.Default.Server;
                _mCon.Username = Settings.Default.UserName;
                _mCon.Password = Settings.Default.Password;
                
                _mCon.UseSSL = true;
                _mCon.UseStartTLS = false;
                _mCon.UseCompression = false;
                _mCon.SocketConnectionType = SocketConnectionType.Direct;

                _mCon.KeepAlive = true;
                _mCon.KeepAliveInterval = 60;

                //_mCon.OnSocketError += OnError;

                _mCon.OnError += OnError;
                _mCon.OnSocketError += OnError;


                _mCon.OnMessage += OnNewMessageArrived;


                _mCon.OnXmppConnectionStateChanged += OnXmppConnectionStateChanged;
                if(IsDebug)
                {
                    _mCon.OnWriteXml += OnWriteXml;
                    _mCon.OnReadXml += OnReadXml;
                }

                _mCon.OnPresence += OnPresence;


                _mCon.OnIq += OnIq;

                _mCon.OnLogin += OnLogin;

                _mCon.Open();
            }
            catch(Exception exception)
            {
                //Log(exception.ToString());
            }
        }

        public static bool IsDebug;


        static void OnError(object sender, Exception ex)
        {
            /*
            try
            {
                //Log(ex.ToString());
                Disconnect();
                Connect();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e.Message);
            }
            */
        }

        private static void Log(string message)
        {
            LogManager.GetLogger("Debug").Info(message + ", Tread " + Thread.CurrentThread.ManagedThreadId);
        }
        static List<string> _msgs = new List<string>();
        private static DateTime? _lastSentTime;
        public static void SendMessage(string message)
        {
            #region unused
            /*
            var mCon = new XmppClientConnection();


            mCon.Resource = null;
            mCon.Priority = 100;
            mCon.Port = 5223;
            mCon.ConnectServer = Settings.Default.ConnectServer;
            mCon.Server = Settings.Default.Server;
            mCon.Username = Settings.Default.UserName;
            mCon.Password = Settings.Default.Password;

            mCon.UseSSL = true;
            mCon.UseStartTLS = false;
            mCon.UseCompression = false;
            mCon.SocketConnectionType = SocketConnectionType.Direct;

            mCon.KeepAlive = true;
            mCon.KeepAliveInterval = 60;

            mCon.Open();
            */
            #endregion
            /*
            if (_lastSentTime != null && DateTime.Now - _lastSentTime < TimeSpan.FromSeconds(30))
            {
                _msgs.Add(message);
                return;
            }
            string all_msg = "";

            foreach (var i in _msgs)
            {
                all_msg += i + Environment.NewLine;
            }
            all_msg += message;

            _msgs.Clear();
            _lastSentTime = DateTime.Now;
            */
            //var m = new Message();
            if(_mCon == null || _msg==null||_to==null)
                return;

            var m = _msg;

            if (ResponceToWeb && m.Html != null)
            {
                m.Html.InnerXml =
                    "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                    message + "</span></span></span></p></body>";
            }
            //*/
            m.Id = Guid.NewGuid().ToString();
            m.To = _to;
            //m.To = new Jid(Settings.Default.Admin, "gmail.com", "");
            m.From = _mCon.MyJID;
            m.Body = message;
            _mCon.Send(m);

            //mCon.Close();
           
            return;
        }

        static ActionType GetAction(string[] body, out object[] pars)
        {
            var actionType = ActionType.None;
            try{ actionType = (ActionType)Enum.Parse(typeof(ActionType), body[0].ToLower(), true); }
            catch
            {
                try
                { actionType = (ActionType)Enum.Parse(typeof(ActionType), Common.TransCode(body[0].ToLower()), true); }
                catch { }
            }

            pars = new object[0];

            if (body.Length > 1)
                pars = body[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return actionType;
        }

        static Jid _to;
        static Message _msg;

        static JEP65Socket _sock = new JEP65Socket();
        private static void OnNewMessageArrived(object sender, Message msg)
        {
            try
            {
                //Log("message arrived");
                if(msg.Body == null) return;
                if(msg.From.Bare == _mCon.MyJID.Bare) return;
                if (msg.Type != MessageType.chat) return;

                if (msg.From.Bare != Settings.Default.Admin)
                {
                    var m = msg;
                    m.To = msg.From;
                    m.From = _mCon.MyJID;
                    m.Body = "I don't know you";

                    if (ResponceToWeb && m.Html != null)
                    {
                        m.Html.InnerXml =
                            "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                            m.Body + "</span></span></span></p></body>";
                    }
                    //*/
                    _mCon.Send(m);

                    return;
                }

                _to = msg.From;
                _msg = msg;

                string[] body = msg.Body.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if(body.Length == 0)
                    body = new string[] { "msg.Body" };

                ActionType actionType = ActionType.None;
                var pars = new object[0];
                Message response;

                if (body[0].ToLower().Equals("i") 
                    || Common.TransCode(body[0].ToLower()).Equals("i"))
                {
                    response = msg;
                    response.To = msg.From;
                    response.From = _mCon.MyJID;
                    response.Body = msg.Type + "," + msg.To;

                    if (ResponceToWeb && response.Html != null)
                    {
                        response.Html.InnerXml =
                            "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                            response.Body + "</span></span></span></p></body>";
                    }
                    //*/
                    _mCon.Send(response);
                    return;
                }

                actionType = GetAction(body, out pars);

                if (actionType == ActionType.None)
                {
                    response = msg;
                    response.To = msg.From;
                    response.From = _mCon.MyJID;
                    response.Body = "unknown command";

                    if (ResponceToWeb && response.Html != null)
                    {
                        response.Html.InnerXml =
                            "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                            response.Body + "</span></span></span></p></body>";
                    }
                    //*/
                    _mCon.Send(response);
                }
                string result = "";
                if(actionType == ActionType.list)
                {
                    var members = typeof(ActionType).GetFields();
                    var res = "";
                    foreach (var fieldInfo in members)
                    {
                        res += fieldInfo.Name + Environment.NewLine;
                    }
                    result = res;
                }
                else if (actionType == ActionType.lf)
                {
                    //_table.ClickBet((decimal)0.08);
                    var copyFile = Application.StartupPath + @"\ApplicationLog" + DateTime.Now.ToString("HHmmss") + ".csv";
                    var zipFile = Application.StartupPath + @"\log" + DateTime.Now.ToString("HHmmss") + ".zip";

                    using (var mail = new SmtpClient(Settings.Default.SmtpServer, 25))
                    {
                        mail.EnableSsl = false;
                        mail.DeliveryMethod = SmtpDeliveryMethod.Network;
                        if (Settings.Default.IsMailAuth)
                        {
                            mail.Credentials = new NetworkCredential(Settings.Default.MailUser, Settings.Default.MailPass);
                        }
                        mail.Timeout = 10000;


                        using (var message = new MailMessage(Settings.Default.MailUser, Settings.Default.Admin))
                        {
                            message.Subject = "poker log " + DateTime.Now.ToString("HH:mm:ss");



                            File.Copy(Application.StartupPath + @"\ApplicationLog.txt", copyFile, true);


                            using (var zip = new ZipFile())
                            {
                                zip.Password = "somepass";
                                zip.AddFile(copyFile, "");

                                zip.Save(zipFile);
                            }
                            message.Attachments.Add(new Attachment(zipFile));
                            mail.Send(message); //, new object());
                        }
                    }
                    File.Delete(zipFile);
                    File.Delete(copyFile);
                    //var tr = new Thread(SendEmail);
                    //tr.IsBackground = true;
                    //tr.Start();
                    

                    //_sock.Address = Settings.Default.ConnectServer;
                    //_sock.Port = 5223;
                    //_sock.Target = msg.To;
                    //_sock.Initiator = msg.From;
                    //_sock.SID = Guid.NewGuid().ToString();
                    //_sock.ConnectTimeout = 5000;
                    //_sock.SyncConnect();

                    //_sock.SendFile(Application.StartupPath + @"\ApplicationLog.txt");
                    result = "log sent";
                }
                else if (actionType == ActionType.GetPics)
                {
                    //TODO
                }
                else if (actionType == ActionType.Log)
                {

                    //LogManager.GetLogger("Debug").Flush(); //TODO Logger flush
                    #region Read Log
                    File.Copy(Application.StartupPath + @"\ApplicationLog.txt", Application.StartupPath + @"\ApplicationLog_rw.txt", true);
                    // Read the file and display it line by line.
                    var file = new StreamReader(Application.StartupPath + @"\ApplicationLog_rw.txt", System.Text.Encoding.UTF8);

                    if (file.BaseStream.Length > 1950)
                    {
                        file.BaseStream.Seek(-1950, SeekOrigin.End);
                    }
                    else if (file.BaseStream.Length > 1024)
                    {
                        file.BaseStream.Seek(-1024, SeekOrigin.End);
                    }
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        result += line + Environment.NewLine;
                    }

                    file.Close();
                    File.Delete(Application.StartupPath + @"\ApplicationLog_rw.txt");
                    #endregion


                    //File.Copy(Application.StartupPath + @"\logs\ApplicationLog.txt", Application.StartupPath + @"\logs\ApplicationLog_rw.txt", true);
                    //var s = File.ReadAllLines(Application.StartupPath + @"\logs\ApplicationLog_rw.txt", System.Text.Encoding.GetEncoding("windows-1251"));
                    //Array.Reverse(s);
                    //File.Delete(Application.StartupPath + @"\logs\ApplicationLog_rw.txt");
                    ////*/

                    //int lineFromDown = 0, linesTotal = 15;

                    //if (pars.Length == 2)
                    //{
                    //    int.TryParse(pars[0].ToString(), out lineFromDown);
                    //    int.TryParse(pars[1].ToString(), out linesTotal);

                    //    linesTotal = Math.Abs(linesTotal);
                    //    lineFromDown = Math.Abs(lineFromDown);

                    //    if (linesTotal > 15)
                    //        linesTotal = 15;

                    //    if (lineFromDown > s.Length - 1 - linesTotal)
                    //        lineFromDown = 0;


                    //}

                    //for (int i = lineFromDown; i <= linesTotal + lineFromDown; i++)
                    //{
                    //    result += s[i] + Environment.NewLine;
                    //}


                    //var ft = new FileTransfer(_mCon, msg.From, Application.StartupPath + @"\logs\ApplicationLog_rw.txt", Settings.Default.Proxy);
                    //ft.Send();
                }
                else
                {


                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is IAction && form.Name == "TableForm")
                            result += ((IAction)form).Perform(actionType, pars) + Environment.NewLine;
                    }
                }
                
                
                /*
                switch (msg.Type)
                {
                    case MessageType.chat:
                        break;
                    case MessageType.error:
                        break;
                    case MessageType.groupchat:
                        break;
                    case MessageType.headline:
                        break;
                    case MessageType.normal:
                        break;
                    default:
                        break;
                }
                */
                response = msg;
                response.To = msg.From;
                response.From = _mCon.MyJID;
                response.Body = result;

                if (ResponceToWeb && response.Html != null)
                {
                    response.Html.InnerXml =
                        "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                        result + "</span></span></span></p></body>";
                }
                //*/
                _mCon.Send(response);
            }
            catch(Exception exception)
            {
                Message response = msg;
                response.To = msg.From;

                response.From = _mCon.MyJID;

                response.Body = exception.ToString();

                if (ResponceToWeb && response.Html != null)
                {
                    response.Html.InnerXml =
                        "<body xmlns=\"http://www.w3.org/1999/xhtml\"><p><span style=\"font-size: small;\"><span style=\"color: #000000;\"><span style=\"font-family: Arial;\">" +
                        exception.ToString() + "</span></span></span></p></body>";
                }
                //*/
                _mCon.Send(response);
            }
        }

        private static void OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            //Log("Xmpp state changed, new state: " + state);
        }

        private static void OnWriteXml(object sender, string xml)
        {
            //Log(xml);
        }

        private static void OnReadXml(object sender, string xml)
        {
            //Log(xml);
        }

        private static void OnPresence(object sender, Presence pres)
        {
            //Log(pres.From.Bare + " entered");
        }

        private static void OnIq(object sender, IQ iq)
        {
            try
            {
                if (iq.Query == null) return;
                if (iq.Type != IqType.get) return;
                if (iq.Query.GetType() == typeof(agsXMPP.protocol.iq.version.Version))
                {
                    VersionIq _iq = new VersionIq();
                    _iq.To = iq.From;
                    _iq.Id = iq.Id;
                    _iq.Type = IqType.result;
                    _iq.Query.Name = "";
                    _iq.Query.Ver = "";
                    _iq.Query.Os = "";
                    _mCon.Send(_iq);
                }
                else
                    if (iq.Query.GetType() == typeof(Time))
                    {
                        TimeIq _iq = new TimeIq();
                        _iq.To = iq.From;
                        _iq.Id = iq.Id;
                        _iq.Type = IqType.result;
                        _iq.Query.Tz = Common.Now.ToString();
                        _mCon.Send(_iq);
                    }
                    else
                        if (iq.Query.GetType() == typeof(Ping))
                        {
                            PingIq _iq = new PingIq();
                            _iq.To = iq.From;
                            _iq.Id = iq.Id;
                            _iq.Type = IqType.result;
                            _mCon.Send(_iq);
                        }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("Debug").Error(e.Message);
            }
        }

        private static void OnLogin(object sender)
        {
            try
            {
                _mCon.SendMyPresence();
                //Log("Logged in");
            }
            catch (Exception e)
            {
                LogManager.GetLogger("Debug").Error(e.Message);
            }
        }
    }
}