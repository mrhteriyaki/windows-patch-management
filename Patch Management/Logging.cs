using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchInstaller
{
    public class Logging
    {
        string _filename;
        public Logging()
        {
            _filename = @"C:\Windows\Temp\PatchManagement-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log";
            Console.WriteLine("Log file at: " + _filename);
            StreamWriter sw = new StreamWriter(_filename);
            sw.WriteLine("MRH Patch Management - Started");
            sw.Close();
        }


        public void WriteLine(string Message)
        {
            string logline = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + Message;
            Console.WriteLine(logline);
            StreamWriter sw = new StreamWriter(_filename,true);
            sw.WriteLine(logline);
            sw.Close();
        }


    }
}
