using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Management
{
    public class RebootManager
    {
        public static void Restart()
        {
            Process rebproc = new();
            rebproc.StartInfo.FileName = @"C:\Windows\System32\shutdown.exe";
            rebproc.StartInfo.Arguments = "/r /t 1";
            rebproc.Start();
        }


        public static bool CheckPendingRestart()
        {
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
