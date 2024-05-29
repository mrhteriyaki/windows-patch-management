using Patch_Management;
using System;
using System.Diagnostics;
using System.Linq;
using static Patch_Management.WindowsPatchManagement;

namespace PatchInstaller
{

    static class Module1
    {

        public static void Main(string[] args)
        {
            WUpdate.DisplayPendingUpdates();

            try
            {
                if (args.Count() == 0)
                {
                    Help();
                    return;
                }

                bool DriverInstall = false;
                bool RebootApproved = false;
                bool SoftwareUpdates = false;

                foreach (string arg in args)
                {
                    if (arg.Equals("?") | arg.ToLower().Equals("help"))
                    {
                        Help();
                        return;
                    }
                    else if (arg.Equals("-drivers"))
                    {
                        DriverInstall = true;
                    }
                    else if (arg.Equals("-reboot"))
                    {
                        RebootApproved = true;
                    }
                    else if (arg.Equals("-updates"))
                    {
                        SoftwareUpdates = true;
                    }
                    else if (arg.Equals("history"))
                    {
                        WUpdateHistory.DisplayHistory();
                        return;
                    }
                    else if (arg.Equals("check"))
                    {
                        WUpdate.DisplayPendingUpdates();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Argument: " + arg);
                        return;
                    }
                }

                if (InstallUpdates(DriverInstall, SoftwareUpdates))
                {
                    Console.WriteLine("Reboot required for updates.");
                    if (RebootApproved)
                    {
                        // Reboot system.
                        Process rebproc = new();
                        rebproc.StartInfo.FileName = @"C:\Windows\System32\shutdown.exe";
                        rebproc.StartInfo.Arguments = "/r /t 1";
                        rebproc.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error installing updates: " + ex.ToString());
                Environment.ExitCode = 1;
            }

            // Potential speed improvement using offline database of updates:  wsusscn2.cab
            // https://stackoverflow.com/questions/27337433/slow-wua-windows-update-api

            // https://support.microsoft.com/en-au/topic/a-new-version-of-the-windows-update-offline-scan-file-wsusscn2-cab-is-available-for-advanced-users-fe433f4d-44f4-28e3-88c5-5b22329c0a08


        }

       

        static void Help()
        {
            Console.WriteLine("Patch Installer - Mitchell Hayden");
            Console.WriteLine("Installs windows updates.");
            Console.WriteLine("Arguments available:");
            Console.WriteLine("-updates     Include Windows Software Updates.");
            Console.WriteLine("-drivers     Include Hardware driver updates.");
            Console.WriteLine("-reboot      Reboot if required when updates completed.");
            Console.WriteLine("history      Show Windows update history for device.");
            Console.WriteLine("check      Check for available updates and show list.");
        }

    }
}