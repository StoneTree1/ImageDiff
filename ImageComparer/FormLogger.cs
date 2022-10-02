using System;
using System.Collections.Generic;
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
            File.Delete($"");
        }

        private static FormLogger instance;
        public static FormLogger Instance => instance ?? (instance = new FormLogger());
        
        public void Log(string messgae)
        {
            File.AppendAllText(LogPath, messgae);
        }
    }
}
