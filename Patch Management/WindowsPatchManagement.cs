using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WUApiLib;

namespace Patch_Management
{

    public class WindowsPatchManagement
    {
        // Microsoft API Docs : https://docs.microsoft.com/en-us/windows/win32/api/wuapi/nn-wuapi-iupdatesearcher

        // Required Refs (add to project)
        // WUApiLib 

        // Additional ref:
        // https://learn.microsoft.com/en-us/windows/win32/wua_sdk/searching--downloading--and-installing-updates


        // Public Shared updatesToDownload As Object
        // Public Shared downloader As Object
        // Public Shared updatesToInstall As Object
        // Public Shared strInput As String
        // Public Shared installer As Object
        // Public Shared installationResult As Object

       

        public static bool InstallUpdates(bool IncludeDrivers = false, bool IncludeSoftware = true) // Return if shutdown required.
        {
            var updateSession = new UpdateSession();
            UpdateSearcher updateSearcher = (UpdateSearcher)updateSession.CreateUpdateSearcher();
            updateSession.ClientApplicationID = "MRH Patch Management"; // appName

            // Fetch available updates.
            // Alternate query = IsInstalled=0 and Type='Software'
            // more info at https://learn.microsoft.com/en-us/windows/win32/api/wuapi/nf-wuapi-iupdatesearcher-search

            // IUpdate Type integer values are 1 = Software, 2 = Driver.
            // https://learn.microsoft.com/en-us/windows/win32/api/wuapi/ne-wuapi-updatetype

            Console.Write("Searching for updates to ");
            if (IncludeDrivers & IncludeSoftware)
            {
                Console.Write("drivers and windows.");
            }
            else if (IncludeDrivers)
            {
                Console.Write("drivers.");
            }
            else if (IncludeSoftware)
            {
                Console.Write("windows.");
            }
            Console.WriteLine();

            ISearchResult searchResult;
            try
            {
                searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching for updates. Exception:" + ex.ToString());
                return false;
            }

            if (searchResult.Updates.Count == 0)
            {
                Console.WriteLine("No Updates available to install.");
                return false;
            }

            Console.WriteLine("Pending Updates:");
            var updateCol = new UpdateCollection();
            for (int I = 0, loopTo = searchResult.Updates.Count - 1; I <= loopTo; I++)
            {
                var update = searchResult.Updates[I];
                if (update.Type == UpdateType.utDriver & IncludeDrivers)
                {
                    // Only install drivers if requested.
                    Console.WriteLine("Driver: " + update);
                    updateCol.Add(update);
                }
                else if (update.Type == UpdateType.utSoftware & IncludeSoftware)
                {
                    // Install software updates unless excluded.
                    Console.WriteLine("Software:" + update);
                    updateCol.Add(update);
                }

            }
            Console.WriteLine();

            if (updateCol.Count == 0)
            {
                // No Updates for installation, skip remaining.
                Console.WriteLine("No updates available.");
                return false;
            }

            // Download Updates.
            Console.WriteLine(Constants.vbCrLf + "Downloading updates.");
            var downloader = updateSession.CreateUpdateDownloader();
            downloader.Updates = updateCol;
            downloader.Download();

            // Console.WriteLine(vbCrLf & "List of downloaded updates:")
            // For I = 0 To searchResult.Updates.Count - 1
            // Dim update As IUpdate = searchResult.Updates.Item(I)
            // If update.IsDownloaded Then
            // Console.WriteLine(I + 1 & "> " & update.Title)
            // End If
            // Next


            Console.WriteLine("Creating Update Installer");
            UpdateInstaller installer = (UpdateInstaller)updateSession.CreateUpdateInstaller();
            installer.Updates = updateCol;

            Console.WriteLine("Installing " + updateCol.Count.ToString() + " updates, please wait this may take a while.");
            var installationResult = installer.Install();

            // Output results of install
            Console.WriteLine("Installation Result: " + ((int)installationResult.ResultCode).ToString());
            Console.WriteLine("Reboot Required: " + installationResult.RebootRequired + Constants.vbCrLf);
            Console.WriteLine("Listing of updates installed and individual installation results:");

            for (int I = 0, loopTo1 = updateCol.Count - 1; I <= loopTo1; I++)
            {
                string ResultStr = ((int)installationResult.GetUpdateResult(I).ResultCode).ToString();
                if ((int)installationResult.GetUpdateResult(I).ResultCode == 2)
                {
                    ResultStr = "Completed (2)";
                }
                else if ((int)installationResult.GetUpdateResult(I).ResultCode == 4)
                {
                    ResultStr = "Failed (4)";
                }
                Console.WriteLine(I + 1 + "> " + updateCol[I] + ", Result Code: " + ResultStr);
            }

            if (installationResult.RebootRequired == true)
            {
                return true;
            }
            return false;
        }

        

        

    }
}