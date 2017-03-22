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
        public static List<ResultItem> SearchFragments(Db db, Db db_post, string query)
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
                    Title = db.Table<OldTmp.WholePost>().Where(f => f.Id == first_id).Select(f => new { f.Title }).First().Title,
                    Score = r.First().Score
                };
                results.Add(item);
            }

            if (!results.Any())
            {
                return SearchLogic.SearchPosts(db_post, db, query);
            }
            else
            {
                return results;
            }
        }

        public static List<ResultItem> SearchPosts(Db db, Db db_answer, string query)
        {

            var results = new List<ResultItem>();
            var interm = new List<IntermResult>();

            //int max_step = db.Table<WholePost>().LastStep();
            //int step = 50;
            //Stopwatch sp = new Stopwatch();
            //sp.Start();
            //for (int i = 0; i <= max_step; i += step)
            //{
            //    var ids = db.Table<WholePost>().Search(f => f.Text, query, i, step).Select(f => new { f.Id });
            //    var answ = db_answer.Table<AnswerFragment>().IntersectListInt(f => f.Id, ids.Select(f => f.Id).ToList()).SelectEntity();
            //    var res = answ.Select(f => new IntermResult { Id = f.Id, Text = Encoding.UTF8.GetString(f.Text), QuestionId = f.Id });
            //    interm.AddRange(res);
            //    if (sp.ElapsedMilliseconds > 1000 || interm.GroupBy(f => f.QuestionId).Count() >= 5)
            //    {
            //        break;
            //    }
            //}

            var ids = db.Table<WholePost>().Search(f => f.Text, query).OrderByDescending(f => f.Votes).Take(5).Select(f => new { f.Id });
            var answ = db_answer.Table<AnswerFragment>().IntersectListInt(f => f.Id, ids.Select(f => f.Id).ToList()).SelectEntity();
            var res = answ.Select(f => new IntermResult { Id = f.Id, Text = Encoding.UTF8.GetString(f.Text), QuestionId = f.Id });
            interm.AddRange(res);

            foreach (var r in interm.GroupBy(f => f.QuestionId).OrderByDescending(f => f.First().Score).Take(5))
            {
                var first_id = r.First().QuestionId;
                var item = new ResultItem()
                {
                    Fragment = r.First().Text,
                    Id = r.First().QuestionId,
                    Title = db_answer.Table<OldTmp.WholePost>().Where(f => f.Id == first_id).Select(f => new { f.Title }).First().Title,
                    Score = -1000 + r.First().Score,
                };
                results.Add(item);
            }

            return results;
        }
    }
}
