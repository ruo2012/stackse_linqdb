using LinqDb;
using MarkdownDeep;
using StackData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Word2vec.Tools;

namespace ImportStack
{
    public class Word2Vec
    {
        public static void WriteSynonyms()
        {
            string db_path = @"C:\Users\Administrator\Documents\stackoverflow\FINAL_DATA";
            Db db_syns = new Db(db_path);

            var path = @"clean.txt";
            var lines = File.ReadAllLines(path).Where(f => !string.IsNullOrEmpty(f)).ToList();
            foreach (var line in lines)
            {
                var parts = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var syn = new Syn()
                {
                    Name = parts[0],
                    Synonyms = parts.Skip(1).Select(f => f).Distinct().Aggregate((a, b) => a + "|" + b)
                };
                db_syns.Table<Syn>().Save(syn);
            }
            db_syns.Dispose();
        }
        static string NonReadable
        {
            get
            {
                return "#~@$^&*=,.!?:;-\"'`()[]<>{}/·–|↑•%°„_\\";
            }
        }
        static void WriteWord2VecTrainingFile()
        {
            Db db = new Db(@"C:\Users\renbo\Desktop\DATA");
            int total = 0;
            using (TextWriter tw = new StreamWriter(@"E:\file\file.txt"))
            {
                int step = 10000;
                for (int i = 0; ; i += step)
                {
                    if (total > 1000000)
                    {
                        break;
                    }
                    StringBuilder sb = new StringBuilder();
                    var qs = db.Table<Question>().BetweenInt(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive).Select(f => new { f.Id, f.Title, f.Body });
                    total += qs.Count();
                    foreach (var q in qs)
                    {
                        var qtext = RemoveMarkdown(Encoding.UTF8.GetString(q.Body)) + Environment.NewLine;
                        sb.Append(qtext);

                        var comments = db.Table<Comment>().Where(f => f.PostId == q.Id).Select(f => new { f.Text });
                        foreach (var comment in comments)
                        {
                            if (comment.Text == null || comment.Text.Count() == 0)
                            {
                                continue;
                            }
                            var ctext = RemoveMarkdown(Encoding.UTF8.GetString(comment.Text)) + Environment.NewLine;
                            sb.Append(" " + ctext);
                        }

                        var answers = db.Table<Answer>().Where(f => f.ParentId == q.Id).Select(f => new { f.Id, f.Body, f.Score });
                        foreach (var answer in answers)
                        {
                            var atext = RemoveMarkdown(Encoding.UTF8.GetString(answer.Body)) + Environment.NewLine;
                            sb.Append(" " + atext);
                            var acomments = db.Table<Comment>().Where(f => f.PostId == answer.Id).SelectEntity();
                            foreach (var comment in acomments)
                            {
                                if (comment.Text == null || comment.Text.Count() == 0)
                                {
                                    continue;
                                }
                                var ctext = RemoveMarkdown(Encoding.UTF8.GetString(comment.Text)) + Environment.NewLine;
                                sb.Append(" " + ctext);
                            }
                        }
                    }
                    var words = sb.ToString().Split();
                    foreach (var w in words)
                    {
                        var word = w.TrimStart("~#@$^&*=,!?:;-\"'`()[]<>{}/·–|↑•%°„_\\ ".ToCharArray()).TrimEnd("~@$^&*=,.!?:;-\"'`()[]<>{}/·–|↑•%°„_\\ ".ToCharArray()).ToLower();
                        if (string.IsNullOrEmpty(word))
                        {
                            continue;
                        }
                        tw.Write(word + " ");
                    }
                }
            }

            db.Dispose();
        }
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
                return body;
            }
        }


        #region training on file
        //from gensim.models import word2vec
        //import logging
        //logging.basicConfig(format= '%(asctime)s : %(levelname)s : %(message)s', level= logging.INFO)
        //sentences = word2vec.Text8Corpus('file.txt')
        //model = word2vec.Word2Vec(sentences, size=200)
        #endregion

        #region getting simmilars
        //dict = {}
        //counter = 0
        //for word, vocab_obj in model.vocab.items():
        // if len(word) > 11:
        //  continue
        // dict[word] = model.most_similar([word], topn=3)
        // counter = counter + 1
        // if counter % 1000 == 0:
        //  print counter
        #endregion

        #region writing file
        //import io
        //with io.open('E:/file/clean.txt', 'w', encoding= "utf-8") as file:
        //for key in dict:
        //file.write(str(key)+" "+str(dict[key][0][0])+" "+str(dict[key][1][0])+" "+str(dict[key][2][0])+"\r\n")
        #endregion
    }
}
