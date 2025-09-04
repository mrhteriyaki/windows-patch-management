using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PatchInstaller
{
    public class ClientReporting
    {
        static readonly string report_config_file = "PatchInstallerReporting.config";
        static string report_server = "";
        static string report_clientid = "";
        static string report_group = "";

        public static void Upload()
        {



        }

        public static void RunReporting(Dictionary<string, string> InstallResults)
        {
            if(!File.Exists(report_config_file))
            {
                return;
            }

            ConfigFile CF = new ConfigFile(report_config_file);
            report_server = CF.GetValue("report_server");
            report_clientid = CF.GetValue("report_clientid");
            report_group = CF.GetValue("report_group");

            if(string.IsNullOrEmpty(report_server))
            {
                Console.WriteLine("Reporting Error - Server Address Missing");
                return;
            }
            Console.WriteLine("Reporting Server set to " + report_server);

            foreach(var IR in InstallResults)
            {
                //Console.WriteLine(IR.Key);
                //Console.WriteLine(IR.Value);
            }

        }


    }
}
