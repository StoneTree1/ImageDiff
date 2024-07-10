using ImageDiff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageComparer
{
    internal class FormLogger
    {
        string LogPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Log.txt";
        public FormLogger()
        {
           // File.Delete($"");
            LogEmiter.LoggingEvent += LogEmit;
        }

        private static FormLogger instance;
        public static FormLogger Instance => instance ?? (instance = new FormLogger());

        public void Log(string messgae)
        {
            File.AppendAllText(LogPath, messgae);
        }



        private static void LogEmit(object sender, ImageDiff.LogEventArgs args)
        {
            if (Debugger.IsAttached)
            {
                //File.AppendAllText("C:\\tmp\\ImageLogging.txt", $"\n{args.Message}");
            }
        }
    }
}
