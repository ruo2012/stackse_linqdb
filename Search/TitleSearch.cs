using LinqDb;
using StackData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    partial class SearchLogic
    {
        public static List<ResultItem> SearchTitles(Db db1, Db db2, string query)
        {
            var bds = new List<Db>() { db1, db2 };
            var result_items = new List<ResultItem>();
            object _lock = new object();
            Parallel.ForEach(bds, db =>
            {
                int count = 0;
                var res = db.Table<WholePost>().Search(f => f.Title, query).OrderByDescending(f => f.Votes).Take(10).Select(f => new { f.Id, f.Title }, out count);
                foreach (var r in res)
                {
                    var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
                    var ri = new ResultItem()
                    {
                        Title = r.Title,
                        Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
                        Id = r.Id
                    };
                    lock (_lock)
                    {
                        result_items.Add(ri);
                    }
                }
            });

            return result_items.OrderByDescending(f => f.Score).Take(5).ToList();
        }
    }

    public class Result
    {
        public long TimeMs { get; set; }
        public long SearchTimeMs { get; set; }
        public long OtherMs { get; set; }
        public int Total { get; set; }
        public int Searched { get; set; }
        public List<ResultItem> Items { get; set; }
    }
    public class ResultItem
    {
        public string Title { get; set; }
        public int Score { get; set; }
        public string Fragment { get; set; }
        public int Id { get; set; }
    }
}
