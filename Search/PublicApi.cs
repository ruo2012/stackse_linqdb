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
        static string p1 = @"C:\Users\Administrator\Documents\stackoverflow\p1";
        static string p2 = @"C:\Users\Administrator\Documents\stackoverflow\p2";
        static string p3 = @"C:\Users\Administrator\Documents\stackoverflow\p3";
        static string p4 = @"C:\Users\Administrator\Documents\stackoverflow\p4";

        static Db db1 { get; set; }
        static Db db2 { get; set; }

        public static void Init()
        {
            if (db1 == null)
            {
                db1 = new Db(p3);
                db2 = new Db(p4);
            }
        }
        public static void Dispose()
        {
            if (db1 != null)
            {
                db1.Dispose();
                db1 = null;
                db2.Dispose();
                db2 = null;
            }
        }
        public static List<ResultItem> SearchTitles(string query)
        {
            Init();
            return SearchLogic.SearchTitles(db1, db2, query);
        }
        public static List<Result> SearchMain(string query, int which)
        {
            Init();
            return SearchLogic.SearchText(db1, db2, which, query);
        }

    }
}
