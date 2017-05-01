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
        static string post_db = @"C:\Users\Administrator\Documents\stackoverflow\INTERM_DATA";

        static Db db { get; set; }
        static Db db_post { get; set; }

        public static void Init()
        {
            if (db == null)
            {
                db = new Db(frag_db);
                db_post = new Db(post_db);
            }
        }
        public static void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
                db_post.Dispose();
                db_post = null;
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
            return SearchLogic.SearchFragments(db, db_post, query);
        }
        public static List<string> GetPossibleQueries(string query)
        {
            Init();
            var res = SearchLogic.GetAllPossibleQueries(db, query);
            var list = res.Item1;
            list.AddRange(res.Item2);
            return list;
        }
    }
}
