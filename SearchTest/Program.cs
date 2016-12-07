using StackData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    class Program
    {
        static void Main(string[] args)
        {
            bool title = false;
            if (title)
            {
                Console.WriteLine("TITLE SEARCH");
                while (true)
                {
                    var query = Console.ReadLine();

                    if (string.IsNullOrEmpty(query))
                    {
                        SearchApi.Dispose();
                        break;
                    }

                    query = query.ToLower().Trim();
                    var results = SearchApi.SearchTitles(query);
                    foreach (var item in results)
                    {
                        Console.WriteLine("{0} - {1}\n{2}\n\n", item.Title, item.Score, item.Fragment.Replace("\r", " ").Replace("\n", " "));
                    }
                }
            }
            else
            {
                Console.WriteLine("MAIN SEARCH");

                while (true)
                {
                    var query = Console.ReadLine();

                    if (string.IsNullOrEmpty(query))
                    {
                        SearchApi.Dispose();
                        break;
                    }

                    query = query.ToLower().Trim();
                    Stopwatch sw_global = new Stopwatch();
                    sw_global.Start();
                    var results = SearchApi.SearchMain(query, 0);
                    foreach (var item in results.SelectMany(f => f.Items).OrderByDescending(f => f.Score).Take(5).ToList())
                    {
                        Console.WriteLine("{0} - {1}\n{2}\n\n", item.Title, item.Score, item.Fragment.Replace("\r", " ").Replace("\n", " "));
                    }
                    sw_global.Stop();
                    if (results.Any())
                    {
                        Console.WriteLine("Time: {0} ms, avg st {1} ms, avg other {2} ms , total {3}, searched {4}" + Environment.NewLine, sw_global.ElapsedMilliseconds, results.Average(f => f.SearchTimeMs), results.Average(f => f.OtherMs), results.Sum(f => f.Total), results.Sum(f => f.Searched));
                    }

                }
            }

        }
    }
}
