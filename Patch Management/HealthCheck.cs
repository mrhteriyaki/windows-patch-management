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
        static bool useLocalTrace = false;

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

        static bool CheckSymbolDepends()
        {
            //Some older versions of windows pull a copy of SymSrv.dll from Windows Defender.
            string modulepath = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\Modules\\WindowsUpdate\\WindowsUpdateLog.psm1";
            if (File.Exists(modulepath))
            {
                foreach (string line in File.ReadAllLines(modulepath))
                {
                    if (line.Contains("$SYMSRV_DLL_PATH = \"$env:ProgramFiles\\Windows Defender\\SymSrv.dll\""))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

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
                File.Delete(file);
            }

            Console.WriteLine("Processing ETL Logs into C:\\Windows\\Temp\\PatchManagement");

            useLocalTrace = CheckSymbolDepends();
            if (useLocalTrace)
            {
                Console.WriteLine("Using local tracerpt mode.");
                Directory.CreateDirectory(@"C:\Windows\Temp\PatchManagement\SymCache");
                Directory.CreateDirectory(@"C:\Windows\Temp\PatchManagement\en-US");

                if (!File.Exists(@"C:\Windows\Temp\PatchManagement\tracerpt.exe"))
                {
                    File.Copy(@"C:\Windows\System32\tracerpt.exe", @"C:\Windows\Temp\PatchManagement\tracerpt.exe");
                }
                if (!File.Exists(@"C:\Windows\Temp\PatchManagement\SymSrv.dll"))
                {
                    File.Copy(@"C:\Program Files\Windows Defender\SymSrv.dll", @"C:\Windows\Temp\PatchManagement\SymSrv.dll");
                }
                if (!File.Exists(@"C:\Windows\Temp\PatchManagement\en-US\tracerpt.exe.mui"))
                {
                    File.Copy(@"C:\Windows\System32\en-US\tracerpt.exe.mui", @"C:\Windows\Temp\PatchManagement\en-US\tracerpt.exe.mui");
                }
                if (!File.Exists(@"C:\Windows\Temp\PatchManagement\DbgHelp.dll"))
                {
                    File.Copy(@"C:\Windows\System32\DbgHelp.dll", @"C:\Windows\Temp\PatchManagement\DbgHelp.dll");
                }


            }

            //Convert each to XML.
            foreach (string etlfile in etlfiles)
            {
                string filename = Path.GetFileName(etlfile);
                Process conProc = new Process();

                conProc.StartInfo.FileName = "C:\\Windows\\System32\\tracerpt.exe";

                if (useLocalTrace)
                {
                    conProc.StartInfo.WorkingDirectory = @"C:\Windows\Temp\PatchManagement\SymCache";
                    conProc.StartInfo.FileName = @"C:\Windows\Temp\PatchManagement\tracerpt.exe";
                }


                conProc.StartInfo.Arguments = etlfile + " -o " + Path.Combine(tmpfolder, filename) + ".xml -of XML -pdb SRV*C:\\Windows\\Temp\\PatchManagement\\SymCache*http://msdl.microsoft.com/download/symbols -i C:\\Windows\\System32\\wuaueng.dll;C:\\Windows\\System32\\wuapi.dll;C:\\Windows\\System32\\wuuhext.dll;C:\\Windows\\System32\\wuuhmobile.dll;C:\\Windows\\System32\\wuautoappupdate.dll;C:\\Windows\\System32\\storewuauth.dll;C:\\Windows\\System32\\wuauclt.exe; -y";
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
                    //Event/System/TimeCreated
                    XmlNode systemNode = eventNode.SelectSingleNode("ns:System", nsmgr);
                    XmlNode timeNode = systemNode.SelectSingleNode("ns:TimeCreated", nsmgr);

                    
                    if(!useLocalTrace)
                    {
                        //Event/EventData
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
                    else
                    {
                        //older version uses debug.
                        //Event/DebugData
                        XmlNode DebugDataNode = eventNode.SelectSingleNode("ns:DebugData", nsmgr);
                        if (DebugDataNode != null)
                        {
                            SW.Write(timeNode.Attributes["SystemTime"].Value);
                            SW.Write(" ");
                            
                            XmlNodeList MessageNode = DebugDataNode.SelectNodes("ns:Message", nsmgr);
                            foreach(XmlNode MsgNode in MessageNode)
                            {
                                SW.Write(MsgNode.InnerText);
                            }

                            SW.WriteLine();
                        }
                        

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
