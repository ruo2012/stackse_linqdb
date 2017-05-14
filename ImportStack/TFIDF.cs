using LinqDb;
using StackData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportStack
{
    public class TFIDF
    {
        static string NonReadable
        {
            get
            {
                return "#~@$^&*=,.!?:;-\"'`()[]<>{}/·–|↑•%°„_\\0123456789";
            }
        }
        public static void CalculateTFIDF()
        {
            string db_path = @"C:\Users\Administrator\Documents\stackoverflow\WORDS_DATA";
            string interm_path = @"C:\Users\Administrator\Documents\stackoverflow\INTERM_DATA";
            Db db_words = new Db(db_path);
            Db db_post = new Db(interm_path);

            int step = 10000;
            int start = db_post.Table<WholePost>().OrderBy(f => f.Id).Take(1).Select(f => new { f.Id }).Select(f => f.Id).First();
            int end = db_post.Table<WholePost>().OrderByDescending(f => f.Id).Take(1).Select(f => new { f.Id }).Select(f => f.Id).First();
            var list = new List<WholePost>();

            int print_counter = 0;
            var dic = new Dictionary<string, int>();
            for (int i = start; ; i += step, print_counter += step)
            {
                list = db_post.Table<WholePost>().BetweenInt(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive).SelectEntity();
                if (!list.Any())
                {
                    if (i < end)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (print_counter % 50000 == 0)
                {
                    Console.WriteLine("phase 1: " + print_counter);
                }
                int total = 0;
                foreach (var p in list)
                {
                    var text = p.Text;
                    var words = new List<string>();
                    foreach (var word in text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var w = word.ToLower().Trim(" .,?()!:;'\"[]".ToCharArray());
                        if (string.IsNullOrEmpty(w) || StopWords.IsStopWord(w) || w.Length > 10 || w.Count(f => NonReadable.Contains(f)) > 3 ||
                           (w.Count(f => NonReadable.Contains(f)) / (double)w.Length > 0.3 && w.Length > 4))
                        {
                            continue;
                        }
                        words.Add(w);
                    }
                    foreach (var w in words.GroupBy(f => f))
                    {
                        if (dic.ContainsKey(w.Key))
                        {
                            dic[w.Key]++;
                        }
                        else
                        {
                            dic[w.Key] = 1;
                        }
                        total++;
                    }
                }
            }


            dic = dic.Where(f => f.Value > 40).ToDictionary(f => f.Key, f => f.Value);

            //calculate tf-idf
            var saved_words = new HashSet<string>();
            int total_docs = db_post.Table<WholePost>().Count();
            step = 10000;
            print_counter = 0;
            int saved_count = 0;
            int not_saved_count = 0;
            for (int i = start; ; i += step, print_counter += step)
            {
                list = db_post.Table<WholePost>().BetweenInt(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive).SelectEntity();
                if (!list.Any())
                {
                    if (i < end)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (print_counter % 50000 == 0)
                {
                    Console.WriteLine("phase 2: " + print_counter);
                }
                var saved = new List<WordTfIdf>();
                foreach (var p in list)
                {
                    var text = p.Text;
                    var words = new List<string>();
                    foreach (var word in text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var w = word.ToLower().Trim(" .,?()!:;'\"[]".ToCharArray());
                        if (string.IsNullOrEmpty(w) || StopWords.IsStopWord(w) || w.Length > 13 || w.Count(f => NonReadable.Contains(f)) > 3 ||
                            (w.Count(f => NonReadable.Contains(f)) / (double)w.Length > 0.3 && w.Length > 4))
                        {
                            continue;
                        }
                        words.Add(w);
                    }
                    foreach (var w in words.GroupBy(f => f))
                    {
                        if (!dic.ContainsKey(w.Key))
                        {
                            continue;
                        }
                        double tf = w.Count() < Math.E ? w.Count() : (Math.E + Math.Log(w.Count()));
                        int wdocs = dic[w.Key];
                        double tmp = total_docs / (double)wdocs;
                        double idf = tmp < Math.E ? tmp : (Math.E + Math.Log(tmp));
                        var tfidf = new WordTfIdf()
                        {
                            QuestionId = p.Id,
                            Word = w.Key,
                            TfIdf = tf * idf
                        };
                        if (tfidf.TfIdf > 17)
                        {
                            saved.Add(tfidf);
                            if (saved.Count() == 5000)
                            {
                                db_words.Table<WordTfIdf>().SaveBatch(saved);
                                saved = new List<WordTfIdf>();
                            }
                            saved_words.Add(tfidf.Word);               
                            saved_count++;
                        }
                        else
                        {
                            not_saved_count++;
                        }
                    }
                }
                db_words.Table<WordTfIdf>().SaveBatch(saved);
                saved = new List<WordTfIdf>();
            }

            //convenient representation
            var saved_data = new List<WordTfIdfData>();
            foreach (var w in saved_words)
            {
                var l = db_words.Table<WordTfIdf>().Where(f => f.Word == w).SelectEntity();
                var data = Utils.TfIdfToData(l);
                var nd = new WordTfIdfData()
                {
                    Word = w,
                    Data = data
                };
                saved_data.Add(nd);
                if (saved_data.Count() == 50)
                {
                    db_words.Table<WordTfIdfData>().SaveBatch(saved_data);
                    saved_data = new List<WordTfIdfData>();
                }
            }
            db_words.Table<WordTfIdfData>().SaveBatch(saved_data);
            saved_data = new List<WordTfIdfData>();

            Console.WriteLine("Done. Saved: " + saved_count + " not saved: " + not_saved_count);
            db_words.Dispose();
            db_post.Dispose();
        }
        
    }
}
