﻿using LinqDb;
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
        public static List<ResultItem> SearchTitles(Db db, Db db_words, string query)
        {
            //var tfidf_dic = new Dictionary<string, Dictionary<int, short>>();
            //HashSet<int> current_set = null;
            //foreach (var w in query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            //{
            //    var d = db_words.Table<WordTfIdfData>().Where(f => f.Word == w).SelectEntity().FirstOrDefault();
            //    if (d == null)
            //    {
            //        current_set = new HashSet<int>();
            //    }
            //    else
            //    {
            //        var data = Utils.TfIdfFromData(d.Data);
            //        tfidf_dic[w] = data.Item1;
            //        if (current_set == null)
            //        {
            //            current_set = data.Item2;
            //        }
            //        else
            //        {
            //            current_set.IntersectWith(data.Item2);
            //        }
            //    }
            //}

            //if (current_set.Any())
            //{
            //    var list = new List<Tuple<int, int>>();
            //    foreach (var id in current_set)
            //    {
            //        int score = 0;
            //        foreach (var w in query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            //        {
            //            score += tfidf_dic[w][id];
            //        }
            //        list.Add(new Tuple<int, int>(id, score));
            //    }

            //    var result_items_tf = new List<ResultItem>();
            //    foreach (var id in list.OrderByDescending(f => f.Item2).Take(10))
            //    {
            //        var item = db.Table<WholePost>().Where(f => f.Id == id.Item1).Select(f => new { f.Id, f.Title, f.Votes }).First();
            //        var afr = db.Table<AnswerFragment>().Where(f => f.Id == id.Item1).Select(f => new { f.Text }).FirstOrDefault();
            //        var ri = new ResultItem()
            //        {
            //            Title = item.Title,
            //            Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
            //            Id = item.Id,
            //            Score = id.Item2 + 1000000//item.Votes + 100
            //        };

            //        result_items_tf.Add(ri);
            //    }

            //    return result_items_tf;
            //}

            //object _lock = new object();
            var result_items = new List<ResultItem>();

            int count = 0;
            var res = db.Table<WholePost>().Search(f => f.Title, query).OrderByDescending(f => f.Votes).Take(200).Select(f => new { f.Id, f.Title, f.Votes }, out count);
            foreach (var r in res)
            {
                var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
                var ri = new ResultItem()
                {
                    Title = r.Title,
                    Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
                    Id = r.Id,
                    Score = /*r.Votes +*/ 100
                };

                result_items.Add(ri);
            }

            //stems
            //var words = query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //if (/*result_items.Count() < 10 &&*/ words.Count() > 1)
            //{
            //    var stems = new List<string>();
            //    foreach (var w in words)
            //    {
            //        stems.Add(Utils.GetStemFromWord(w));
            //    }
            //    var s_query = stems.Aggregate((a, b) => a + " " + b);

            //    res = db.Table<WholePost>().Search(f => f.TitleStem, s_query).OrderByDescending(f => f.Votes).Take(10).Select(f => new { f.Id, f.Title, f.Votes }, out count);
            //    foreach (var r in res)
            //    {
            //        var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
            //        var ri = new ResultItem()
            //        {
            //            Title = r.Title,
            //            Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
            //            Id = r.Id,
            //            Score = r.Votes + 100
            //        };
            //        result_items.Add(ri);
            //    }
            //}
            //var queries = GetAllPossibleQueries(db, query);

            ////full
            //int count = 0;
            //var res = db.Table<WholePost>().Search(f => f.Title, queries.Item1[0]).OrderByDescending(f => f.Votes).Take(10).Select(f => new { f.Id, f.Title, f.Votes }, out count);
            //foreach (var r in res)
            //{
            //    var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
            //    var ri = new ResultItem()
            //    {
            //        Title = r.Title,
            //        Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
            //        Id = r.Id,
            //        Score = (r.Votes < 3 ? 1 : (int)Math.Log(r.Votes)) + 10000
            //    };
            //    lock (_lock)
            //    {
            //        result_items.Add(ri);
            //    }
            //}

            //Parallel.ForEach(queries.Item1.Skip(1), q =>
            //{
            //    int count_s = 0;
            //    var res_s = db.Table<WholePost>().Search(f => f.Title, q).OrderByDescending(f => f.Votes).Take(10).Select(f => new { f.Id, f.Title, f.Votes }, out count_s);
            //    foreach (var r in res_s)
            //    {
            //        var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
            //        var ri = new ResultItem()
            //        {
            //            Title = r.Title,
            //            Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
            //            Id = r.Id,
            //            Score = r.Votes < 3 ? 1 : (int)Math.Log(r.Votes)
            //        };
            //        lock (_lock)
            //        {
            //            result_items.Add(ri);
            //        }
            //    }
            //});
            ////stem
            //foreach (var q in queries.Item2)
            //{
            //    int count_st = 0;
            //    var res_st = db.Table<WholePost>().Search(f => f.TitleStem, q).OrderByDescending(f => f.Votes).Take(10).Select(f => new { f.Id, f.Title, f.Votes }, out count_st);
            //    foreach (var r in res_st)
            //    {
            //        var afr = db.Table<AnswerFragment>().Where(f => f.Id == r.Id).Select(f => new { f.Text }).FirstOrDefault();
            //        var ri = new ResultItem()
            //        {
            //            Title = r.Title,
            //            Fragment = afr != null ? Encoding.UTF8.GetString(afr.Text) : "",
            //            Id = r.Id,
            //            Score = (r.Votes < 3 ? 1 : (int)Math.Log(r.Votes)) + 5000
            //        };
            //        lock (_lock)
            //        {
            //            result_items.Add(ri);
            //        }
            //    }
            //}
            foreach (var inter in result_items)
            {
                inter.Score += TitleScore(inter.Title);
            }

            foreach (var inter in result_items)
            {
                inter.Score += GetFragmentScore(inter.Title, query);
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
            foreach (var inter in result_items)
            {
                foreach (var w in query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (dic.ContainsKey(w) && dic[w].ContainsKey(inter.Id))
                    {
                        inter.Score += dic[w][inter.Id];
                    }
                }
            }

            return result_items.GroupBy(f => f.Title).Select(f => f.First()).OrderByDescending(f => f.Score).Take(5).ToList();
        }

        static int TitleScore(string title)
        {
            return 20 - (title.Length / 5);
        }

        public static Tuple<List<string>, List<string>> GetAllPossibleQueries(Db db, string query)
        {
            var full = new List<string>();
            var words = query.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            //synonyms
            var syns = new Dictionary<string, List<string>>();
            foreach (var w in words)
            {
                var val = db.Table<Syn>().Where(f => f.Name == w).SelectEntity().FirstOrDefault();
                if (val != null)
                {
                    syns[w] = new List<string>() { w };
                    syns[w].AddRange(val.Synonyms.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
                }
            }
            if (syns.Any())
            {
                full.AddRange(GetQueries(syns, words));
                full = full.Distinct().OrderBy(f => f.Length).Take(15).ToList();
            }
            
            full.Insert(0, query);
            return new Tuple<List<string>, List<string>>(full.Distinct().ToList(), new List<string>());
        }

        static List<string> GetQueries(Dictionary<string, List<string>> syns, List<string> parts)
        {
            var res = new List<string>();
            if (parts.Count() == 1)
            {
                if (!syns.ContainsKey(parts[0]))
                {
                    res = new List<string>() { parts[0] };
                }
                else
                {
                    res.AddRange(syns[parts[0]]);
                }
            }
            else
            {
                var others = GetQueries(syns, parts.Skip(1).ToList());
                if (!syns.ContainsKey(parts[0]))
                {
                    foreach (var p in others)
                    {
                        res.Add(parts[0] + " " + p);
                    }
                }
                else
                {
                    foreach (var w in syns[parts[0]])
                    {
                        foreach (var p in others)
                        {
                            res.Add(w + " " + p);
                        }
                    }
                }
            }

            var dic = res.ToDictionary(f => f, z => z.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).OrderBy(f => f).Aggregate((a, b) => a + " " + b));
            return dic.GroupBy(f => f.Value).Select(f => f.First()).Select(f => f.Key).ToList();
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
