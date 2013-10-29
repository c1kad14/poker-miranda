using System;
using System.Diagnostics;
using System.Reflection;

namespace LogLoader
{
    public class Ex
    {
        public static void Report(Exception ex)
        {
            var tip = Assembly.GetEntryAssembly().GetType("miranda.Program");
            var method = tip.GetMethod("LogError");

            method.Invoke(null, new object[] {ex});
            //Program.ShowBaloon(ex.Message);
            //LogManager.GetLogger("Application").Error(ex);
        }

        public static void Info(string msg)
        {
            
            var tip = Assembly.GetEntryAssembly().GetType("miranda.Program");
            var method = tip.GetMethod("LogInfo");

            method.Invoke(null, new object[] {msg});
            
            //Program.ShowBaloon(ex.Message);
            //LogManager.GetLogger("Application").Error(ex);
        }
    }
}