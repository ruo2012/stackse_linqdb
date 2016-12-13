using LinqDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    public class SearchApi
    {
        static string frag_db = @"C:\Users\Administrator\Documents\stackoverflow\FINAL_DATA";

        static Db db { get; set; }

        public static void Init()
        {
            if (db == null)
            {
                db = new Db(frag_db);
            }
        }
        public static void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
        public static List<ResultItem> SearchTitles(string query)
        {
            Init();
            return SearchLogic.SearchTitles(db, query);
        }
        public static List<ResultItem> SearchMain(string query)
        {
            Init();
            return SearchLogic.SearchFragments(db, query);
        }

    }
}
