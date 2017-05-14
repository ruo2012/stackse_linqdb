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
        public static List<ResultItem> SearchFragments(Db db, /*Db db_post,*/ Db db_words, string query)
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
                if (sp.ElapsedMilliseconds > 1000 || interm.GroupBy(f => f.QuestionId).Count() >= 1000)
                {
                    break;
                }
            }
            foreach (var inter in interm)
            {
                inter.Score += /*5000 +*/ GetFragmentScore(inter.Text, query);
            }

            //tfidf
            var dic = new Dictionary<string, Dictionary<int, short>>();
            foreach (var w in query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var d = db_words.Table<WordTfIdfData>().Where(f => f.Word == w).SelectEntity().FirstOrDefault();
                if (d == null)
                {
                    continue;
                }
                var data = Utils.TfIdfFromData(d.Data);
                dic[w] = data.Item1;
            }
            foreach (var inter in interm)
            {
                foreach (var w in query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (dic.ContainsKey(w) && dic[w].ContainsKey(inter.Id))
                    {
                        inter.Score += dic[w][inter.Id];
                    }
                }
            }

            foreach (var r in interm.GroupBy(f => f.QuestionId).OrderByDescending(f => f.OrderByDescending(z => z.Score).First().Score).Take(5))
            {
                var q = r.OrderByDescending(z => z.Score).First();
                var first_id = q.QuestionId;
                var item = new ResultItem()
                {
                    Fragment = q.Text,
                    Id = q.QuestionId,
                    Title = db.Table<WholePost>().Where(f => f.Id == first_id).Select(f => new { f.Title }).First().Title,
                    Score = q.Score
                };
                results.Add(item);
            }

            if (!results.Any())
            {
                //return SearchLogic.SearchPosts(db_post, db, query);
                return SearchLogic.SearchSubQueries(db, db_words, query);
            }
            else
            {
                return results;
            }
        }

        public static List<ResultItem> SearchSubQueries(Db db, Db db_words, string query)
        {
            var results = new List<ResultItem>();
            var words = query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Take(5).ToList();
            var other_queries = new List<string>();
            for (int i = 0; i < words.Count(); i++)
            {
                var new_list = new List<string>();
                for (int j = 0; j < words.Count(); j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    new_list.Add(words[j]);
                }
                var nq = new_list.Aggregate((a, b) => a + " " + b);
                other_queries.Add(nq);
            }
            foreach (var q in other_queries)
            {
                var tmp = SearchLogic.SearchTitles(db, db_words, q);
                results.AddRange(tmp);
            }
            foreach (var r in results)
            {
                r.Score += -50000;
            }
            return results;
        }
        //public static List<ResultItem> SearchPosts(Db db, Db db_answer, string query)
        //{

        //    var results = new List<ResultItem>();
        //    var interm = new List<IntermResult>();

        //    var ids = db.Table<WholePost>().Search(f => f.Text, query).OrderByDescending(f => f.Votes).Take(5).Select(f => new { f.Id });
        //    var answ = db_answer.Table<AnswerFragment>().IntersectListInt(f => f.Id, ids.Select(f => f.Id).ToList()).SelectEntity();
        //    var res = answ.Select(f => new IntermResult { Id = f.Id, Text = Encoding.UTF8.GetString(f.Text), QuestionId = f.Id });
        //    interm.AddRange(res);

        //    foreach (var inter in interm)
        //    {
        //        inter.Score += GetFragmentScore(inter.Text, query);
        //    }

        //    foreach (var r in interm.GroupBy(f => f.QuestionId).OrderByDescending(f => f.OrderByDescending(z => z.Score).First().Score).Take(5))
        //    {
        //        var q = r.OrderByDescending(z => z.Score).First();
        //        var first_id = q.QuestionId;
        //        var item = new ResultItem()
        //        {
        //            Fragment = q.Text,
        //            Id = q.QuestionId,
        //            Title = db_answer.Table<WholePost>().Where(f => f.Id == first_id).Select(f => new { f.Title }).First().Title,
        //            Score = -50000 + q.Score
        //        };
        //        results.Add(item);
        //    }

        //    return results;
        //}

        static int GetFragmentScore(string fragment, string query)
        {
            var qs = query.ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (qs.Count() < 2)
            {
                return 0;
            }
            var indexes = new List<int>();
            var lf = fragment.ToLower();
            foreach (var w in qs)
            {
                int index = lf.IndexOf(w);
                if (index >= 0)
                {
                    //Console.WriteLine(w+": "+lf+" "+index);
                    indexes.Add(index);
                }
            }
            if (indexes.Count() <= 1)
            {
                return -1000;
            }
            int score = 0;
            score = (qs.Count() - indexes.Count()) * fragment.Length * 4;
            //penalty for bad order
            for (int i = 1; i < indexes.Count(); i++)
            {
                if (indexes[i] < indexes[i - 1])
                {
                    score += 100;
                }
            }
            int min = indexes.Min();
            //Console.WriteLine(min);
            for (int i = 0; i < indexes.Count; i++)
            {
                score += (indexes[i] - min);
            }

            int max = indexes.Max();
            //Console.WriteLine(max);
            for (int i = 0; i < indexes.Count; i++)
            {
                score += (max - indexes[i]);
            }


            return -1 * score;
        }
    }
}
