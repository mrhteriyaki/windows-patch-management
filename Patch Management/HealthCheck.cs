using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



namespace Patch_Management
{
    public class HealthCheck
    {


        static readonly string[] errorCodes = {
            "80070032", //Feature update failure
            "80248014", //Download Error - possible TLS issue, check CA updates and HTTP TCP 80 access. HTTPS://slscr.update.microsoft.com
            
            "8024402C", //Download Error
                       //'Library download error'
                       //SendRequestWithAuthRetry using proxy failed for HTTPS://slscr.update.microsoft.com/SLS/
                       //possible local store corruption, clear C:\Windows\SoftwareDistribution

        };


        //Generate Log.

        //C:\Windows\System32\tracerpt.exe


        public static void GetWindowsUpdateLog(string outputFile)
        {
            //Get ETL file list.
            Console.WriteLine("Checking for ETL logs in C:\\Windows\\Logs\\WindowsUpdate");
            string[] etlfiles = Directory.GetFiles("C:\\Windows\\Logs\\WindowsUpdate", "*.etl", SearchOption.TopDirectoryOnly);
            Array.Sort(etlfiles, StringComparer.OrdinalIgnoreCase);


            string tmpfolder = "C:\\Windows\\Temp\\PatchManagement";
            if (!Directory.Exists(tmpfolder))
            {
                Directory.CreateDirectory(tmpfolder);
            }

            //Remove any existing xml files.
            foreach (string file in Directory.GetFiles(tmpfolder, "*.xml"))
            {
                System.IO.File.Delete(file);
            }

            Console.WriteLine("Processing ETL Logs into C:\\Windows\\Temp\\PatchManagement");

            //Convert each to XML.
            foreach (string etlfile in etlfiles)
            {
                string filename = Path.GetFileName(etlfile);
                Process conProc = new Process();
                conProc.StartInfo.FileName = "C:\\Windows\\System32\\tracerpt.exe";
                conProc.StartInfo.Arguments = etlfile + " -o " + Path.Combine(tmpfolder, filename) + ".xml -of XML";
                conProc.StartInfo.UseShellExecute = false;
                conProc.StartInfo.RedirectStandardOutput = true;

                conProc.Start();
                conProc.WaitForExit();

            }

            Console.WriteLine("Building log events.");

            StreamWriter SW = new StreamWriter(outputFile);

            foreach (string xmlData in Directory.GetFiles(tmpfolder, "*.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlData);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/win/2004/08/events/event");

                XmlNodeList eventNodes = xmlDoc.SelectNodes("//ns:Event", nsmgr);

                foreach (XmlNode eventNode in eventNodes)
                {
                    XmlNode systemNode = eventNode.SelectSingleNode("ns:System", nsmgr);
                    XmlNode timeNode = systemNode.SelectSingleNode("ns:TimeCreated", nsmgr);



                    XmlNode EventDataNode = eventNode.SelectSingleNode("ns:EventData", nsmgr);
                    if (EventDataNode != null)
                    {
                        SW.Write(timeNode.Attributes["SystemTime"].Value);
                        SW.Write(" ");

                        XmlNodeList dataNodes = EventDataNode.SelectNodes("ns:Data", nsmgr);
                        foreach (XmlNode dataNode in dataNodes)
                        {
                            if (dataNode.Attributes["Name"].Value.Equals("Info"))
                            {
                                SW.Write(dataNode.InnerText);
                            }
                        }

                        SW.WriteLine();
                    }


                }

            }
            SW.Close();
            Console.WriteLine(outputFile);
            CheckErrors(outputFile);

        }



        public static void CheckErrors(string logPath)
        {
            string errorSearch = "*FAILED* [";

            foreach (string line in File.ReadAllLines(logPath))
            {
                if (line.StartsWith(errorSearch))
                {
                    Console.WriteLine(line);
                }
                else
                {
                    foreach (string ec in errorCodes)
                    {
                        if (line.Contains(ec))
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
            }
        }

    }
}
