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
    partial class SearchLogic
    {
        public static List<Result> SearchText(Db db1, Db db2, int which, string query)
        {

            Stopwatch sw_global = new Stopwatch();
            sw_global.Start();
            var tasks = new List<Task>();
            var bds = new List<Db>();
            if (which == 0)
            {
                bds.Add(db1);
            }
            else
            {
                bds.Add(db2);
            }
            var results = new List<Result>();
            object _r_lock = new object();

            Parallel.ForEach(bds, db =>
            //foreach(var db in bds)
            {
                Stopwatch sw_total = new Stopwatch();
                sw_total.Start();
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var res = db.Table<WholePost>().Search(f => f.Text, query).Select(f => new { f.Id });
                int count = res.Count;

                sw.Stop();
                long search_time = sw.ElapsedMilliseconds;
                sw.Reset();
                sw.Start();
                Dictionary<int, int> score = new Dictionary<int, int>();
                Dictionary<int, int> fragments = new Dictionary<int, int>();
                var words = GetDistinctWordsForsearch(query);
                int interval = 100;
                int maxms = 1500;
                int searched = 0;
                for (int i = 0; i < res.Count + interval; i += interval)
                {
                    var tmp = res.Skip(i).Take(i + interval).ToList();
                    if (!tmp.Any())
                    {
                        break;
                    }
                    searched = i + interval;

                    List<int> hashes = new List<int>();
                    var qids = tmp.Select(z => z.Id).ToList();
                    foreach (var id in qids)
                    {
                        foreach (var w in words)
                        {
                            var hash = (w + ":" + id).GetHashCode();
                            hashes.Add(hash);
                        }
                    }
                    var hr = db.Table<FragmentWord>().Intersect(f => f.WordQuestionId, hashes)
                                                     .Select(f => new { f.WordQuestionId, f.FragmentsId });

                    foreach (var id in qids)
                    {
                        var list = new List<List<int>>();
                        foreach (var w in words)
                        {
                            var hash = (w + ":" + id).GetHashCode();
                            var tmpr = hr.Where(f => f.WordQuestionId == hash).FirstOrDefault();
                            if (tmpr == null)
                            {
                                score[id] = 0;
                                fragments[id] = 0;
                                continue;
                            }
                            var fr = new List<int>();
                            for (int j = 0; j < tmpr.FragmentsId.Count(); j += 4)
                            {
                                int fnr = BitConverter.ToInt32(new byte[4] { tmpr.FragmentsId[j], tmpr.FragmentsId[j + 1], tmpr.FragmentsId[j + 2], tmpr.FragmentsId[j + 3] }, 0);
                                fr.Add(fnr);
                            }
                            list.Add(fr);
                        }
                        var val = GetScore(list);
                        score[id] = val.Key;
                        fragments[id] = val.Value;
                    }
                    if (sw.ElapsedMilliseconds > maxms)
                    {
                        break;
                    }
                }

                sw.Stop();
                sw_total.Stop();
                var result = new Result()
                {
                    Items = new List<ResultItem>(),
                    TimeMs = sw_total.ElapsedMilliseconds,
                    SearchTimeMs = search_time,
                    OtherMs = sw.ElapsedMilliseconds,
                    Total = res.Count,
                    Searched = searched
                };

                if (!score.Any())
                {
                    return;
                }
                var maxs = score.Max(f => f.Value);
                var r = score.Where(f => f.Value == maxs);
                foreach (var id in r)
                {
                    var frag_id = fragments[id.Key];
                    var frag = db.Table<PostFragment>().Where(f => f.Id == frag_id).Select(f => new { f.QuestionId, f.Text }).FirstOrDefault();
                    var ri = new ResultItem()
                    {
                        Title = db.Table<WholePost>().Where(f => f.Id == id.Key).Select(f => new { f.Title }).First().Title,
                        Fragment = Encoding.UTF8.GetString(frag.Text),
                        Score = id.Value,
                        Id = frag.QuestionId
                    };
                    result.Items.Add(ri);
                }
                foreach (var item in result.Items)
                {
                    item.Score = item.Score*1000 + (10000 - GetFragmentScore(item, words));
                }
                result.Items = result.Items.OrderByDescending(f => f.Score).Take(5).ToList();
                lock (_r_lock)
                {
                    results.Add(result);
                }
            });
            //}
            sw_global.Stop();
            return results;
        }

        static int GetFragmentScore(ResultItem item, List<string> query)
        {
            var indexes = new List<int>();
            var lf = item.Fragment.ToLower();
            foreach (var w in query)
            {
                int index = lf.IndexOf(w);
                if (index >= 0)
                {
                    indexes.Add(index);
                }
            }

            if (!indexes.Any())
            {
                return Int32.MaxValue;
            }
            int score = 0;
            int min = indexes.Min();
            for (int i = 0; i < indexes.Count; i++)
            {
                score += (indexes[i] - min);
            }

            int max = indexes.Max();
            for (int i = 0; i < indexes.Count; i++)
            {
                score += -1 * (indexes[i] - max);
            }

            return score;
        }

        static KeyValuePair<int, int> GetScore(List<List<int>> list)
        {
            int score = 1;


            var ol = list.OrderByDescending(f => f.Count).ToList();
            var biggest = ol.Take(1).First();
            var fragment_id = biggest.First();
            var chs = new HashSet<int>(biggest);
            foreach (var l in ol.Skip(1))
            {
                var ch = new HashSet<int>(l);
                chs.IntersectWith(ch);
                if (!chs.Any())
                {
                    break;
                }
                else
                {
                    fragment_id = chs.First();
                    score++;
                    //var text = db1.Table<PostFragment>().Where(f => f.Id == fragment_id).SelectEntity();

                }
            }

            int max_score = score;
            score = 1;
            int last_frag = 0;
            for (int i = 0; i < 10; i++)
            {
                ol = list.OrderBy(a => Guid.NewGuid()).ToList();
                biggest = ol.Take(1).First();
                chs = new HashSet<int>(biggest);
                foreach (var l in ol.Skip(1))
                {
                    var ch = new HashSet<int>(l);
                    chs.IntersectWith(ch);
                    if (!chs.Any())
                    {
                        break;
                    }
                    else
                    {
                        last_frag = chs.First();
                        score++;
                    }
                }

                if (score > max_score)
                {
                    fragment_id = last_frag;
                    max_score = score;
                }

                score = 1;
                last_frag = 0;
            }

            return new KeyValuePair<int, int>(max_score, fragment_id);
        }

        static List<string> GetDistinctWordsForsearch(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return new List<string>();
            }

            var res = new List<string>(val.Split().Where(f => !string.IsNullOrEmpty(f)));
            return res.Select(f => f.ToLower()).Distinct().ToList();
        }
    }
}
