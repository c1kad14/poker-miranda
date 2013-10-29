using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;

namespace miranda
{
    public static class Program
    {

        private static NotifyIcon _notifyIcon;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

           
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _notifyIcon = new NotifyIcon();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(TableForm));
            _notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            _notifyIcon.Text = "Poker, build on " +  new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd-MM-yyyy HH:mm:ss");;
            _notifyIcon.Visible = true;
            

            Application.Run(new TableForm());
        }

        public static void ShowBaloon(string msg, ToolTipIcon tip)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ShowBalloonTip(5000, "Ошибка", string.IsNullOrEmpty(msg) ? "-": msg, tip);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowBaloon(((Exception)e.ExceptionObject).Message, ToolTipIcon.Error);
            //_notifyIcon.ShowBalloonTip(5000, "Ошибка", ((Exception)e.ExceptionObject).Message, ToolTipIcon.Error);
            LogManager.GetLogger("Application").Error(e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowBaloon(e.Exception.Message, ToolTipIcon.Error);
            //_notifyIcon.ShowBalloonTip(5000, "Ошибка", e.Exception.Message, ToolTipIcon.Error);
            LogManager.GetLogger("Application").Error(e.Exception);
        }

        public static void LogError(Exception ex)
        {
            ShowBaloon(ex.Message, ToolTipIcon.Error);
            //_notifyIcon.ShowBalloonTip(5000, "Ошибка", e.Exception.Message, ToolTipIcon.Error);
            LogManager.GetLogger("Application").Error(ex);
        }

        public static void LogInfo(string msg)
        {
            //if (!Debugger.IsAttached)
            {
                ShowBaloon(msg, ToolTipIcon.Info);
            }
            //_notifyIcon.ShowBalloonTip(5000, "Ошибка", e.Exception.Message, ToolTipIcon.Error);
            LogManager.GetLogger("Application").Info(msg);
        }
        
    }
}
