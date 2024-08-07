using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace Patch_Management
{
    public class HealthCheck
    {


        static readonly string[] errorCodes = {
            "80070032", //Feature update failure
            "80248014" //Download Error - possible TLS issue, check CA updates and HTTP TCP 80 access. HTTPS://slscr.update.microsoft.com
        };


        //Generate Log.

        //C:\Windows\System32\tracerpt.exe


        public static void GetWindowsUpdateLog()
        {
            //Get ETL file list.
            string[] etlfiles = Directory.GetFiles("C:\\Windows\\Logs\\WindowsUpdate", "*.etl",SearchOption.TopDirectoryOnly);

            string tmpfolder = "C:\\Windows\\Temp\\PatchManagement";
            if (!Directory.Exists(tmpfolder))
            {
                Directory.CreateDirectory(tmpfolder);
            }
            
            //Remove any existing xml files.
            foreach(string file in Directory.GetFiles(tmpfolder,"*.xml"))
            {
                File.Delete(file);
            }

            //Convert each to XML.
            foreach(string etlfile in etlfiles)
            {
                string filename = Path.GetFileName(etlfile);
                Process conProc = new Process();
                conProc.StartInfo.FileName = "C:\\Windows\\System32\\tracerpt.exe";
                conProc.StartInfo.Arguments = etlfile + " -o " + Path.Combine(tmpfolder,filename) + ".xml -of XML";
                conProc.StartInfo.UseShellExecute = false;
                conProc.StartInfo.RedirectStandardOutput = true;
                conProc.Start();
                conProc.WaitForExit();
            }


            //Get Data from Each XML
            //non functional xml parse
            foreach (string xmlData in Directory.GetFiles(tmpfolder,"*.xml"))
            {
                Console.WriteLine(xmlData);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlData);

                XmlSerializer serializer = new XmlSerializer(typeof(Events));

                using (XmlNodeReader nodeReader = new XmlNodeReader(xmlDoc.DocumentElement))
                {
                    Events events = (Events)serializer.Deserialize(nodeReader);
                    foreach (var eventLog in events.EventList)
                    {
                        Console.WriteLine($"Data: {eventLog.EventData.Data.Value}");
                    }
                }

            }
            

        }


        [XmlRoot("Events")]
        public class Events
        {
            [XmlElement("Event")]
            public List<EventLog> EventList { get; set; }
        }



        public class EventLog
        {
            [XmlElement("EventData")]
            public EventData EventData { get; set; }

        }

        public class EventData
        {
            [XmlElement("Data")]
            public Data Data { get; set; }
        }

        public class Data
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlText]
            public string Value { get; set; }
        }

    }
}
