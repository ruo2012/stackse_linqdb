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
        public static void CalculateTFIDF()
        {
            string db_path = @"C:\Users\Administrator\Documents\stackoverflow\WORDS_DATA";
            string interm_path = @"C:\Users\Administrator\Documents\stackoverflow\INTERM_DATA";
            Db db_words = new Db(db_path);
            Db db_post = new Db(interm_path);

            int step = 10000;
            int start = db_post.Table<WholePost>().OrderBy(f => f.Id).Select(f => new { f.Id }).Select(f => f.Id).First();
            int end = db_post.Table<WholePost>().OrderByDescending(f => f.Id).Select(f => new { f.Id }).Select(f => f.Id).First();
            var list = new List<WholePost>();

            for (int i = start; ; i += step)
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
                if (i % 100000 == 0)
                {
                    Console.WriteLine("phase 1: " + i);
                }
                var words_q_total = new Dictionary<string, WordDocFreq>();
                foreach (var p in list)
                {
                    var text = p.Text;
                    foreach (var word in text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var w = word.ToLower().Trim(" .,?()!:;".ToCharArray());
                        if (string.IsNullOrEmpty(w))
                        {
                            continue;
                        }
                        var key = w + " " + p.Id;
                        if (words_q_total.ContainsKey(key))
                        {
                            words_q_total[key].Count++;
                        }
                        else
                        {
                            words_q_total[key] = new WordDocFreq()
                            {
                                QuestionId = p.Id,
                                Word = w,
                                Count = 1
                            };
                        }
                    }
                }
                db_words.Table<WordDocFreq>().SaveBatch(words_q_total.Select(f => f.Value).ToList());
            }

            //calculate tf-idf
            int total_docs = db_post.Table<WholePost>().Count();
            step = 20000;
            for(int i=0; ; i += step)
            {
                var words = db_words.Table<WordDocFreq>().BetweenInt(f => f.Id, i, i + step, BetweenBoundaries.FromInclusiveToExclusive).SelectEntity();
                if (!words.Any())
                {
                    break;
                }
                if (i % 100000 == 0)
                {
                    Console.WriteLine("phase 2: " + i);
                }
                var res_list = new List<WordTfIdf>();
                foreach (var w in words)
                {
                    
                    double tf = w.Count < Math.E ? w.Count : (Math.E + Math.Log(w.Count));
                    int wdocs = db_words.Table<WordDocFreq>().Where(f => f.Word == w.Word).Count();
                    double tmp = total_docs / (double)wdocs;
                    double idf = tmp < Math.E ? tmp : (Math.E + Math.Log(tmp));
                    var tfidf = new WordTfIdf()
                    {
                        QuestionId = w.QuestionId,
                        Word = w.Word,
                        TfIdf = tf*idf
                    };
                    res_list.Add(tfidf);
                }
                db_words.Table<WordTfIdf>().SaveBatch(res_list);
            }

            db_words.Dispose();
            db_post.Dispose();
        }
    }
}
