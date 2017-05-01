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
            int what = Convert.ToInt32(Console.In.ReadLine());
            if (what == 0)
            {
                Console.WriteLine("POSSIBLE QUERIES");
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
                    var results = SearchApi.GetPossibleQueries(query);
                    foreach (var item in results)
                    {
                        Console.WriteLine("{0}", item);
                    }
                    sw_global.Stop();
                    Console.WriteLine("Time: {0} ms, total {1}" + Environment.NewLine, sw_global.ElapsedMilliseconds, results.Count());
                }
            }
            else if (what == 1)
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
                    Stopwatch sw_global = new Stopwatch();
                    sw_global.Start();
                    var results = SearchApi.SearchTitles(query);
                    foreach (var item in results)
                    {
                        Console.WriteLine("{0} - {1}\n{2}\n\n", item.Title, item.Score, item.Fragment.Replace("\r", " ").Replace("\n", " "));
                    }
                    sw_global.Stop();
                    Console.WriteLine("Time: {0} ms, total {1}" + Environment.NewLine, sw_global.ElapsedMilliseconds, results.Count());
                }
            }
            else if (what == 2)
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
                    var results = SearchApi.SearchMain(query);
                    foreach (var item in results.OrderByDescending(f => f.Score).Take(5).ToList())
                    {
                        Console.WriteLine("{0} - {1}\n{2}\n\n", item.Title, item.Score, item.Fragment.Replace("\r", " ").Replace("\n", " "));
                    }
                    sw_global.Stop();
                    Console.WriteLine("Time: {0} ms, total {1}" + Environment.NewLine, sw_global.ElapsedMilliseconds, results.Count());

                }
            }

        }
    }
}
