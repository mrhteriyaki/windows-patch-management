using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Patch_Management
{
    public class WUpdateHistory
    {
        public string Title;
        public string InstallDate;
        public string Description;
        public string Revision;
        public string Category;
        public int InstallResult;

        public static List<WUpdateHistory> GetUpdateHistory()
        {
            // Returns list of installed windows updates.
            Type MicrosoftUpdateSessionTp = Type.GetTypeFromProgID("Microsoft.Update.Session");
            dynamic updateSession = Activator.CreateInstance(MicrosoftUpdateSessionTp);

            //var updateSession = Interaction.CreateObject("Microsoft.Update.Session");
            updateSession.ClientApplicationID = "MRH Patch Management Library";

            var updateSearcher = ((dynamic)updateSession).CreateupdateSearcher();
            int UpdateCount = ((dynamic)updateSession).CreateupdateSearcher().GetTotalHistoryCount;
            var searchResult = updateSearcher.QueryHistory(0, UpdateCount);

            // Search result object api doc: https://learn.microsoft.com/en-us/windows/win32/api/wuapi/nn-wuapi-iupdatehistoryentry

            List<WUpdateHistory> WHistory = new List<WUpdateHistory>();
            for (int I = 0, loopTo = (searchResult.Count - 1); I <= loopTo; I++)
            {
                var update = searchResult.Item(I);
                WUpdateHistory tWUpdateHistory = new WUpdateHistory();
                tWUpdateHistory.Title = update.Title;
                DateTime UDT = (DateTime)update.Date;
                tWUpdateHistory.InstallDate = UDT.ToString("yyyy-MM-dd HH:mm:ss"); // format for SQL.
                tWUpdateHistory.Description = update.Description;
                tWUpdateHistory.InstallResult = update.HResult;

                var UpdateIdentity = update.UpdateIdentity;
                int RevInt = UpdateIdentity.RevisionNumber;
                tWUpdateHistory.Revision = RevInt.ToString();

                var Categories = update.Categories;
                string CatString = "";
                foreach (dynamic Cat in (IEnumerable)Categories)
                {
                    CatString = CatString + Cat.Name + ",";
                }

                if (CatString.EndsWith(","))
                {
                    CatString = CatString.Substring(0, CatString.Length - 1);
                }

                tWUpdateHistory.Category = CatString;


                // Skip updates without title.
                if (string.IsNullOrEmpty(tWUpdateHistory.Title))
                {
                    continue;
                }

                WHistory.Add(tWUpdateHistory);
            }

            return WHistory;
        }

        public static void DisplayHistory()
        {
            Console.WriteLine("Install Date,HResult Code,HResult Description,Revision,Category,Title,Description");
            foreach (WUpdateHistory WUpdate in GetUpdateHistory())
            {
                Console.WriteLine(
                    WUpdate.InstallDate + "," +
                    WUpdate.InstallResult.ToString("X") + "," +
                    HRESULT.GetDescription(WUpdate.InstallResult) + "," +
                    WUpdate.Revision + "," +
                    WUpdate.Category + "," +
                    WUpdate.Title + "," +
                    WUpdate.Description);
            }
        }

    }
}
