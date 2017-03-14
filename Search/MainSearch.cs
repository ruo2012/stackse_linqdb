using LinqDb;
using StackData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    public class IntermResult
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public int Score { get; set; }
    }
    partial class SearchLogic
    {
        public static List<ResultItem> SearchFragments(Db db, string query)
        {

            var results = new List<ResultItem>();
            var interm = new List<IntermResult>();

            int max_step = db.Table<PostFragment>().LastStep();
            int step = 50;
            Stopwatch sp = new Stopwatch();
            sp.Start();
            for (int i = 0; i <= max_step; i += step)
            {
                var res = db.Table<PostFragment>().Search(f => f.Text, query, i, step)
                            .Select(f => new { f.Id, f.Text, f.QuestionId })
                            .Select(f => new IntermResult { Id = f.Id, Text = f.Text, QuestionId = f.QuestionId });
                interm.AddRange(res);
                if (sp.ElapsedMilliseconds > 1000 || interm.GroupBy(f => f.QuestionId).Count() >= 10)
                {
                    break;
                }
            }
            foreach (var r in interm.GroupBy(f => f.QuestionId).OrderByDescending(f => f.First().Score).Take(5))
            {
                var first_id = r.First().QuestionId;
                var item = new ResultItem()
                {
                    Fragment = r.First().Text,
                    Id = r.First().QuestionId,
                    Title = db.Table<WholePost>().Where(f => f.Id == first_id).Select(f => new { f.Title }).First().Title
                };
                results.Add(item);
            }

            return results;
        }
    }
}
