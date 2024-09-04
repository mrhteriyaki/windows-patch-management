using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PatchInstaller;
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

        static readonly string clientName = "MRH Patch Management";

        public static void InstallUpdate(int Index)
        {

            var updateSession = new UpdateSession();
            UpdateSearcher updateSearcher = (UpdateSearcher)updateSession.CreateUpdateSearcher();
            updateSession.ClientApplicationID = clientName;

            ISearchResult searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");

            UpdateCollection updateCol = new UpdateCollection();
            updateCol.Add(searchResult.Updates[Index]);
            Console.WriteLine(searchResult.Updates[Index].Title);

            var downloader = updateSession.CreateUpdateDownloader();
            downloader.Updates = updateCol;
            downloader.Download();

            UpdateInstaller installer = (UpdateInstaller)updateSession.CreateUpdateInstaller();
            installer.Updates = updateCol;

            IInstallationResult installationResult = installer.Install();

            // Output results of install
            Console.WriteLine("Installation Result: " + ResultCode.GetCodeValue((int)installationResult.ResultCode));
            if(installationResult.RebootRequired)
            {
                Console.WriteLine("Restart required to complete install.");
            }
            

            
        }


        public static bool InstallUpdates(bool IncludeDrivers = false, bool IncludeSoftware = true) // Return if shutdown required.
        {
            Logging logger = new Logging();

            var updateSession = new UpdateSession();
            UpdateSearcher updateSearcher = (UpdateSearcher)updateSession.CreateUpdateSearcher();
            updateSession.ClientApplicationID = clientName; // appName

            // Fetch available updates.
            // Alternate query = IsInstalled=0 and Type='Software'
            // more info at https://learn.microsoft.com/en-us/windows/win32/api/wuapi/nf-wuapi-iupdatesearcher-search

            // IUpdate Type integer values are 1 = Software, 2 = Driver.
            // https://learn.microsoft.com/en-us/windows/win32/api/wuapi/ne-wuapi-updatetype

            string msg = "Searching for updates to ";
            if (IncludeDrivers & IncludeSoftware)
            {
                msg = msg + "drivers and windows.";
            }
            else if (IncludeDrivers)
            {
                msg = msg + "drivers.";
            }
            else if (IncludeSoftware)
            {
                msg = msg + "windows.";
            }
            logger.WriteLine(msg);

            ISearchResult searchResult;
            try
            {
                searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");
            }
            catch (Exception ex)
            {
                logger.WriteLine("Error searching for updates. Exception:" + ex.ToString());
                return false;
            }

            if (searchResult.Updates.Count == 0)
            {
                logger.WriteLine("No Updates available to install.");
                return false;
            }

            logger.WriteLine("Pending Updates:");
            UpdateCollection updateCol = new UpdateCollection();
            for (int I = 0, loopTo = searchResult.Updates.Count - 1; I <= loopTo; I++)
            {
                IUpdate update = searchResult.Updates[I];
                if (update.Type == UpdateType.utDriver & IncludeDrivers)
                {
                    // Only install drivers if requested.
                    logger.WriteLine("Driver: " + update.Title);
                    updateCol.Add(update);
                }
                else if (update.Type == UpdateType.utSoftware & IncludeSoftware)
                {
                    // Install software updates unless excluded.
                    logger.WriteLine("Software:" + update.Title);
                    updateCol.Add(update);
                }

            }
            Console.WriteLine();

            if (updateCol.Count == 0)
            {
                // No Updates for installation, skip remaining.
                logger.WriteLine("No updates available.");
                return false;
            }

            // Download Updates.
            logger.WriteLine("Downloading updates.");
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


            logger.WriteLine("Creating Update Installer");
            UpdateInstaller installer = (UpdateInstaller)updateSession.CreateUpdateInstaller();
            installer.Updates = updateCol;

            logger.WriteLine("Installing " + updateCol.Count.ToString() + " updates, please wait this may take a while.");
            IInstallationResult installationResult = installer.Install();

            // Output results of install
            logger.WriteLine("Installation Result: " + ((int)installationResult.ResultCode).ToString());
            logger.WriteLine("Reboot Required: " + installationResult.RebootRequired.ToString());
            logger.WriteLine("Listing of updates installed and individual installation results:");

            for (int I = 0, loopTo1 = updateCol.Count - 1; I <= loopTo1; I++)
            {
                string ResultStr = ResultCode.GetCodeValue((int)installationResult.GetUpdateResult(I).ResultCode);
                
                IUpdate update = (IUpdate)updateCol[I];
                logger.WriteLine(I + 1 + "> " + update.Title + ", Result Code: " + ResultStr);
            }

            if (installationResult.RebootRequired == true)
            {
                return true;
            }
            return false;
        }





    }
}