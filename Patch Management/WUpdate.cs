using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WUApiLib;

namespace Patch_Management
{
    public class WUpdate
    {
        public string Title;
        public int Type; // 1 = Software, 2 = Driver.
        public int Downloaded;
        public int Product;

        public static void DisplayPendingUpdates()
        {
            var updateSession = new UpdateSession();
            UpdateSearcher updateSearcher = (UpdateSearcher)updateSession.CreateUpdateSearcher();

            // Alternate query = IsInstalled=0 and Type='Software'
            ISearchResult searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");
            Console.WriteLine("Checking for Updates:");
            for (int I = 0, loopTo = (searchResult.Updates.Count - 1); I <= loopTo; I++)
            {
                IUpdate update = (IUpdate)(searchResult).Updates[I];
                Console.WriteLine(I.ToString() + ": " + update.Title);
            }
        }
    }




}
