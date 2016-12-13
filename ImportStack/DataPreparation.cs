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
        public static void MakeSearchableData(string whole_path, string interm_path, string final_path, int start, int total)
        {
            var whole_db = new Db(whole_path);
            var interm_db = new Db(interm_path);
            var final_db = new Db(final_path);

            int current = 0;
            var list = new List<WholePost>();
            var posts = new List<WholePost>();
            var answs = new List<AnswerFragment>();
            int i = whole_db.Table<Question>().OrderBy(f => f.Id).Skip(start).Take(1).Select(f => new { f.Id }).First().Id;
            int fin = whole_db.Table<Question>().OrderByDescending(f => f.Id).Take(1).Select(f => new { f.Id }).First().Id;
            for (; i < fin; i++)
            {
                StringBuilder text = new StringBuilder();
                var q = whole_db.Table<Question>()/*.Intersect(f => f.Id, new List<int> { i })*/.Where(f => f.Id == i).Select(f => new { f.Id, f.Body, f.Title, f.Score }).FirstOrDefault();
                if (q == null)
                {
                    continue;
                }
                current++;
                if (current > total)
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

                var comments = whole_db.Table<Comment>()/*.Intersect(f => f.PostId, new List<int> { q.Id })*/.Where(f => f.PostId == q.Id).Select(f => new { f.Text });
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
                var answers = whole_db.Table<Answer>()/*.Intersect(f => f.ParentId, new List<int> { q.Id })*/.Where(f => f.ParentId == q.Id).Select(f => new { f.Id, f.Body, f.Score });
                foreach (var answer in answers)
                {
                    var atext = RemoveMarkdown(Encoding.UTF8.GetString(answer.Body)) + Environment.NewLine;
                    adic.Add(new KeyValuePair<int, string>(answer.Score, atext));
                    text.Append(atext);
                    var acomments = whole_db.Table<Comment>()/*.Intersect(f => f.PostId, new List<int> { answer.Id })*/.Where(f => f.PostId == answer.Id).SelectEntity();
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
                    answs.Add(fragment);
                }
                else
                {
                    var ba = adic.OrderByDescending(f => f.Key).FirstOrDefault();
                    var fragment = new AnswerFragment();
                    fragment.Id = q.Id;
                    var at = ba.Value.Length > 300 ? ba.Value.Substring(0, 300) : ba.Value;
                    fragment.Text = Encoding.UTF8.GetBytes(at);
                    answs.Add(fragment);
                }
                post.Text = Encoding.UTF8.GetBytes(text.ToString());
                posts.Add(post);

                if (posts.Count() > 5000)
                {
                    var interm_posts = new List<WholePost>();
                    foreach (var p in posts)
                    {
                        var ip = new WholePost()
                        {
                            Id = p.Id,
                            Text = p.Text
                        };
                        interm_posts.Add(ip);
                        p.Text = null;
                    }
                    interm_db.Table<WholePost>().SaveBatch(interm_posts);
                    final_db.Table<WholePost>().SaveBatch(posts);
                    posts = new List<WholePost>();
                    final_db.Table<AnswerFragment>().SaveBatch(answs);
                    answs = new List<AnswerFragment>();
                }
            }

            var interm_posts_ = new List<WholePost>();
            foreach (var p in posts)
            {
                var ip = new WholePost()
                {
                    Id = p.Id,
                    Text = p.Text
                };
                interm_posts_.Add(ip);
                p.Text = null;
            }
            interm_db.Table<WholePost>().SaveBatch(interm_posts_);
            final_db.Table<WholePost>().SaveBatch(posts);
            final_db.Table<AnswerFragment>().SaveBatch(answs);
            final_db.Dispose();
            interm_db.Dispose();
            whole_db.Dispose();
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

        public static void MakeFragments(string interm_path, string final_path)
        {
            var interm_db = new Db(interm_path);
            var final_db = new Db(final_path);
            List<PostFragment> batch = new List<PostFragment>();
            int step = 10000;
            int total = 0;
            int start = interm_db.Table<WholePost>().OrderBy(f => f.Id).Take(1).Select(f => new { f.Id }).First().Id;
            int fin = interm_db.Table<WholePost>().OrderByDescending(f => f.Id).Take(1).Select(f => new { f.Id }).First().Id;
            for (int i = start; i <= fin; i += step)
            {
                var res = interm_db.Table<WholePost>()
                                   .Between(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive)
                                   .Select(f => new { f.Id, f.Text });
                if (!res.Any())
                {
                    continue;
                }

                foreach (var post in res)
                {
                    var fragments = GetFragments(Encoding.UTF8.GetString(post.Text));
                    foreach (var f in fragments)
                    {
                        batch.Add(new PostFragment()
                        {
                            QuestionId = post.Id,
                            Text = f
                        });
                    }
                }
                final_db.Table<PostFragment>().SaveBatch(batch);
                total += res.Count;
                Console.WriteLine(total);
                batch = new List<PostFragment>();
            }
            final_db.Dispose();
            interm_db.Dispose();
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
