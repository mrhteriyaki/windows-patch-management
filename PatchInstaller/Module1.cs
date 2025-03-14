﻿using Patch_Management;
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

            try
            {
                if (args.Count() == 0)
                {
                    Help();
                    return;
                }

                Console.WriteLine("Patch Management - Mitchell Hayden");

                bool DriverInstall = false;
                bool RebootApproved = false;
                bool SoftwareUpdates = false;
                bool PreviewUpdates = false;
                bool scriptMode = false;

                //Arguments that can be combined.
                foreach (string arg in args)
                {
                    //single commands run and execute.

                    if (arg.Equals("?") | arg.ToLower().Equals("help"))
                    {
                        Help();
                        return;
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
                    else if (arg.Equals("health"))
                    {
                        HealthCheck.GetWindowsUpdateLog(@"C:\Windows\Temp\PatchManagement\WindowsUpdate.log");
                        return;
                    }

                    //Combine commands.

                    else if (arg.Equals("-update"))
                    {
                        SoftwareUpdates = true;
                    }
                    else if (arg.Equals("-drivers"))
                    {
                        DriverInstall = true;
                    }
                    else if (arg.Equals("-reboot"))
                    {
                        RebootApproved = true;
                    }
                    else if (arg.Equals("-select"))
                    {
                        int index = int.Parse(args[1]);
                        InstallUpdate(index);
                        return;
                    }
                    else if(arg.Equals("-preview"))
                    {
                        PreviewUpdates = true;
                    }
                    else if(arg.Equals("-script"))
                    {
                        scriptMode = true;
                    }

                    //Invalid arguments.
                    else
                    {
                        Console.WriteLine("Invalid Argument: " + arg);
                        return;
                    }
                }

                if (RebootManager.CheckPendingRestart())
                {
                    Console.WriteLine("A pending restart has been detected.");
                }
                else
                {
                    Console.WriteLine("No pending restarts have been detected.");
                }
                Console.WriteLine();


                if(SoftwareUpdates || DriverInstall)
                {
                    if (InstallUpdates(DriverInstall, SoftwareUpdates, PreviewUpdates, scriptMode))
                    {
                        Console.WriteLine("Reboot required for updates.");
                        if (RebootApproved)
                        {
                            Console.WriteLine("Restarting System");
                            RebootManager.Restart();
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Install complete.");
                    }
                }

                //Check for PendingReboot Flags.
                if (RebootApproved && RebootManager.CheckPendingRestart())
                {
                    Console.WriteLine("System is pending reboot - restarting");
                    RebootManager.Restart();
                    return;
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
            Console.WriteLine("Patch Installer Help");
            Console.WriteLine("Install parameters available:");
            Console.WriteLine("-update      Include Windows Software Updates.");
            Console.WriteLine("-drivers     Include Hardware driver updates.");
            Console.WriteLine("-preview     Include Preview Updates.");
            Console.WriteLine("-reboot      Reboot if required when updates completed.");
            Console.WriteLine("-select x    Install single update where X is the index number available in the 'check' list.");
            Console.WriteLine("-script      Reduced status info in console output for remote script execution.");

            Console.WriteLine();
            Console.WriteLine("Reporting commands available:");
            Console.WriteLine("history      Show Windows update history for device.");
            Console.WriteLine("check        Check for available updates and show list.");
            Console.WriteLine("health       Writes windows update logs to disk and checks for errors.");
        }

    }
}