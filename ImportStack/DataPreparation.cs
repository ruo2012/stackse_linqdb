using LinqDb;
using MarkdownDeep;
using StackData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ImportStack
{
    class DataPreparation
    {
        public static void MakeSearchableData(string path, string p1, string p2, string p3, string p4, int start, int total)
        {
            var db = new Db(path);
            var db1 = new Db(p1);
            var db2 = new Db(p2);
            var db3 = new Db(p3);
            var db4 = new Db(p4);

            int ctotal = 0;
            int current = 0;
            var list = new List<WholePost>();
            var dic = new Dictionary<int, List<WholePost>>() { { 0, new List<WholePost>() }, { 1, new List<WholePost>() }, { 2, new List<WholePost>() }, { 3, new List<WholePost>() } };
            var answs = new Dictionary<int, List<AnswerFragment>>() { { 0, new List<AnswerFragment>() }, { 1, new List<AnswerFragment>() }, { 2, new List<AnswerFragment>() }, { 3, new List<AnswerFragment>() } };
            for (int i = 0; ; i++)
            {
                StringBuilder text = new StringBuilder();
                var q = db.Table<Question>().Where(f => f.Id == i).Select(f => new { f.Id, f.Body, f.Title, f.Score }).FirstOrDefault();
                if (q == null)
                {
                    continue;
                }
                current++;
                if (current < start)
                {
                    continue;
                }
                ctotal++;
                if (ctotal > total)
                {
                    break;
                }
                if (current % 10000 == 0)
                {
                    Console.WriteLine("Current: {0}, errors: {1}", current, err_count);
                }

                var post = new WholePost();
                post.Id = q.Id;
                post.Title = q.Title;
                post.Votes = q.Score;
                var qtext = RemoveMarkdown(Encoding.UTF8.GetString(q.Body)) + Environment.NewLine;
                text.Append(qtext);

                var comments = db.Table<Comment>().Where(f => f.PostId == q.Id).Select(f => new { f.Text });
                foreach (var comment in comments)
                {
                    if (comment.Text == null || comment.Text.Count() == 0)
                    {
                        continue;
                    }
                    var ctext = RemoveMarkdown(Encoding.UTF8.GetString(comment.Text)) + Environment.NewLine;
                    text.Append(ctext);
                }

                var adic = new List<KeyValuePair<int, string>>();
                var answers = db.Table<Answer>().Where(f => f.ParentId == q.Id).Select(f => new { f.Id, f.Body, f.Score });
                foreach (var answer in answers)
                {
                    var atext = RemoveMarkdown(Encoding.UTF8.GetString(answer.Body)) + Environment.NewLine;
                    adic.Add(new KeyValuePair<int, string>(answer.Score, atext));
                    text.Append(atext);
                    var acomments = db.Table<Comment>().Where(f => f.PostId == answer.Id).SelectEntity();
                    foreach (var comment in acomments)
                    {
                        if (comment.Text == null || comment.Text.Count() == 0)
                        {
                            continue;
                        }
                        var ctext = RemoveMarkdown(Encoding.UTF8.GetString(comment.Text)) + Environment.NewLine;
                        text.Append(ctext);
                    }
                }

                if (!adic.Any())
                {
                    var fragment = new AnswerFragment();
                    fragment.Id = q.Id;
                    var at = qtext.Length > 300 ? qtext.Substring(0, 300) : qtext;
                    fragment.Text = Encoding.UTF8.GetBytes(at);
                    answs[post.Id % 4].Add(fragment);
                }
                else
                {
                    var ba = adic.OrderByDescending(f => f.Key).FirstOrDefault();
                    var fragment = new AnswerFragment();
                    fragment.Id = q.Id;
                    var at = ba.Value.Length > 300 ? ba.Value.Substring(0, 300) : ba.Value;
                    fragment.Text = Encoding.UTF8.GetBytes(at);
                    answs[post.Id % 4].Add(fragment);
                }
                post.Text = text.ToString();
                dic[post.Id % 4].Add(post);

                if (dic.Sum(f => f.Value.Count()) > 5000)
                {
                    db1.Table<WholePost>().SaveBatch(dic[0]);
                    db2.Table<WholePost>().SaveBatch(dic[1]);
                    db3.Table<WholePost>().SaveBatch(dic[2]);
                    db4.Table<WholePost>().SaveBatch(dic[3]);
                    dic = new Dictionary<int, List<WholePost>>() { { 0, new List<WholePost>() }, { 1, new List<WholePost>() }, { 2, new List<WholePost>() }, { 3, new List<WholePost>() } }; 
                    db1.Table<AnswerFragment>().SaveBatch(answs[0]);
                    db2.Table<AnswerFragment>().SaveBatch(answs[1]);
                    db3.Table<AnswerFragment>().SaveBatch(answs[2]);
                    db4.Table<AnswerFragment>().SaveBatch(answs[3]);
                    answs = new Dictionary<int, List<AnswerFragment>>() { { 0, new List<AnswerFragment>() }, { 1, new List<AnswerFragment>() }, { 2, new List<AnswerFragment>() }, { 3, new List<AnswerFragment>() } };
                }
            }

            db1.Table<WholePost>().SaveBatch(dic[0]);
            db2.Table<WholePost>().SaveBatch(dic[1]);
            db3.Table<WholePost>().SaveBatch(dic[2]);
            db4.Table<WholePost>().SaveBatch(dic[3]);

            db1.Table<AnswerFragment>().SaveBatch(answs[0]);
            db2.Table<AnswerFragment>().SaveBatch(answs[1]);
            db3.Table<AnswerFragment>().SaveBatch(answs[2]);
            db4.Table<AnswerFragment>().SaveBatch(answs[3]);

            db1.Dispose();
            db2.Dispose();
            db3.Dispose();
            db4.Dispose();
            db.Dispose();
        }

        static int err_count { get; set; }
        static string RemoveMarkdown(string body)
        {
            try
            {
                var md = new Markdown();

                var html = md.Transform(body);

                html = html.Replace(">", "> ");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                string dtext = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);
                return Regex.Replace(dtext, @"\s+", " ").Trim();
            }
            catch (Exception)
            {
                err_count++;
                return body;
            }
        }

        public static void MakeFragments(string p1, string p2, string p3, string p4)
        {
            var db1 = new Db(p1);
            var db2 = new Db(p2);
            var db3 = new Db(p3);
            var db4 = new Db(p4);
            var dbs = new List<Db>() { db1, db2, db3, db4 };

            foreach (var db_target in dbs)
            {
                List<PostFragment> batch = new List<PostFragment>();
                int step = 10000;
                int total = 0;
                int start = db_target.Table<WholePost>().OrderBy(f => f.Id).Take(1).Select(f => new { f.Id }).First().Id;
                for (int i = start; ; i += step)
                {
                    var res = db_target.Table<WholePost>()
                                       .Between(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive)
                                       .Select(f => new { f.Id, f.Text });
                    if (!res.Any())
                    {
                        break;
                    }

                    foreach (var post in res)
                    {
                        var fragments = GetFragments(post.Text);
                        foreach (var f in fragments)
                        {
                            batch.Add(new PostFragment()
                            {
                                QuestionId = post.Id,
                                Text = Encoding.UTF8.GetBytes(f)
                            });
                        }
                    }
                    db_target.Table<PostFragment>().SaveBatch(batch);
                    total += res.Count;
                    Console.WriteLine(total);
                    batch = new List<PostFragment>();
                }
                db_target.Dispose();
            }
        }

        public static List<string> GetFragments(string text)
        {
            var res = new List<string>();
            var textw = text.Split(" \t\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int window = 50;
            for (int i = 0; i < textw.Count(); i += window / 2)
            {
                var nf = textw.Skip(i).Take(window);
                res.Add(nf.Aggregate((a, b) => a + " " + b));
            }
            return res.Where(f => !string.IsNullOrEmpty(f)).ToList();
        }

        public static void MakeFragmentWords(string p1, string p2, string p3, string p4)
        {
            var db1 = new Db(p1);
            var db2 = new Db(p2);
            var db3 = new Db(p3);
            var db4 = new Db(p4);
            var dbs = new List<Db>() { db1, db2, db3, db4 };

            foreach (var db_target in dbs)
            {
                Dictionary<int, FragmentWord> batch = new Dictionary<int, FragmentWord>();
                int step = 10000;
                int total = 0;
                int add = 0;
                for (int i = 0; ; i += (step + add))
                {
                    var res = db_target.Table<PostFragment>()
                                       .Between(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive)
                                       .Select(f => new { f.Id, f.QuestionId, f.Text });
                    if (!res.Any())
                    {
                        break;
                    }

                    int max_id = res.Max(z => z.Id);
                    int lastq_id = res.Where(z => z.Id == max_id).First().QuestionId;
                    var more = db_target.Table<PostFragment>().Where(f => f.QuestionId == lastq_id).Select(f => new { f.Id, f.QuestionId, f.Text }).Where(f => f.Id > max_id).ToList();
                    res.AddRange(more);
                    add = more.Count;

                    foreach (var post in res)
                    {
                        //tmp
                        //if (db_target.Table<FragmentWord>().Where(f => f.QId == post.QuestionId).Select(f => new { f.Id }).Any())
                        //{
                        //    continue;
                        //}
                        var text = Encoding.UTF8.GetString(post.Text);
                        var words = GetDistinctWords(text);
                        foreach (var w in words)
                        {
                            var key = (w + ":" + post.QuestionId).GetHashCode();
                            if (!batch.ContainsKey(key))
                            {
                                var f = new FragmentWord()
                                {
                                    FragmentsId = BitConverter.GetBytes(post.Id).ToArray(),
                                    WordQuestionId = key,
                                    QId = post.QuestionId
                                };
                                batch[key] = f;
                            }
                            else
                            {
                                var current = batch[key].FragmentsId.ToList();
                                current.AddRange(BitConverter.GetBytes(post.Id));
                                batch[key].FragmentsId = current.ToArray();
                            }
                        }
                    }
                    db_target.Table<FragmentWord>().SaveBatch(batch.Select(f => f.Value).ToList());
                    total += res.Count;
                    Console.WriteLine(total);
                    batch = new Dictionary<int, FragmentWord>();
                }

                db_target.Dispose();
            }
        }

        public static string AsciiNonReadable
        {
            get
            {
                return "~#@$^&*=,.!?:;-\"'`()[]<>{}/·–|↑•%°„_\\ ";
            }
        }

        static List<string> GetDistinctWords(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return new List<string>();
            }

            var res = new List<string>(val.Split().Where(f => !string.IsNullOrEmpty(f)));
            var more = new List<string>(val.Trim(AsciiNonReadable.ToArray())
                                              .Split(AsciiNonReadable.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                                              .Select(f => f.Trim())
                                              .Where(f => !string.IsNullOrEmpty(f) && !AsciiNonReadable.Contains(f.Trim())));
            res.AddRange(more);
            return res.Select(f => f.ToLower()).Distinct().ToList();
        }
    }
}
