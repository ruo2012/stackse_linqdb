using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class NodeController : Controller
    {
        //[HttpPost]
        //public void IndexLinks()
        //{
        //    var body = DecompressBody(Request);
        //    var links = JsonConvert.DeserializeObject<List<Link>>(body);
        //    ThreadPool.QueueUserWorkItem(f =>
        //        {
        //            foreach (var l in links)
        //            {
        //                try
        //                {
        //                    Indexing.IncrementRefs(l.Href);
        //                    if (!string.IsNullOrEmpty(l.Text))
        //                    {
        //                        Indexing.PutLinks(l.Href, l.Text);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    MyTable.LogInfo("Node ERROR trying to put link ", ex, true);
        //                }
        //            }
        //        });
        //}

        //[HttpPost]
        //public void IndexThese()
        //{
        //    var body = DecompressBody(Request);
        //    var urls = JsonConvert.DeserializeObject<List<string>>(body);
        //    ThreadPool.QueueUserWorkItem(f =>
        //    {
        //        lock (LinkAcceptor._lock)
        //        {
        //            LinkAcceptor.Links.AddRange(urls);
        //        }
        //    });
        //}

        //[HttpPost]
        //public void PutSynonyms()
        //{
        //    var body = DecompressBody(Request);
        //    var data = JsonConvert.DeserializeObject<SynonymsData>(body);

        //    var lines = data.Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var line in lines)
        //    {
        //        var parts = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
        //        string key = "";
        //        foreach (var w in Utils.Util.GetDistinctWords(parts[0].ToLower()))
        //        {
        //            key += w.Key + " ";
        //        }
        //        key = key.Trim();

        //        string val = "";
        //        foreach (var s in parts[1].Split(",".ToCharArray()))
        //        {
        //            string cv = "";
        //            foreach (var w in Utils.Util.GetDistinctWords(s.ToLower()))
        //            {
        //                cv += w.Key + " ";
        //            }
        //            val += cv.Trim() + ",";
        //        }
        //        AzureStorage.AzurePutString(key, val.Trim(",".ToCharArray()), WhichDB.SynonymsDB);
        //    }
        //}

        //[HttpGet]
        //public string GetLinkNumber()
        //{
        //    lock (LinkAcceptor._lock)
        //    {
        //        return LinkAcceptor.Links.Count().ToString();
        //    }
        //}

        //[HttpGet]
        //public string GetStats()
        //{
        //    var total = MyTable.GetStatsCrawler(Const.TotalCrawls);
        //    var failed = MyTable.GetStatsCrawler(Const.FailedCrawls);
        //    return (total != null ? total.Value : "0") + "|" + (failed != null ? failed.Value : "0");
        //}

        //[HttpGet]
        //public string GetNodesLog()
        //{
        //    var res = MyTable.GetQueryLog(50);
        //    foreach (var r in res)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    var data = JsonConvert.SerializeObject(res);
        //    return Utils.Util.Compress(data);
        //}

        //[HttpGet]
        //public string GetNodesSlow()
        //{
        //    var res = MyTable.GetQuerySlow();
        //    foreach (var r in res)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    var data = JsonConvert.SerializeObject(res);
        //    return Utils.Util.Compress(data);
        //}

        //[HttpGet]
        //public string GetNodesErrors()
        //{
        //    var res = MyTable.GetErrors(50);
        //    foreach (var r in res)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    var data = JsonConvert.SerializeObject(res);
        //    return Utils.Util.Compress(data);
        //}

        [HttpPost]
        [ValidateInput(false)]
        public string SearchNode(string data_json)
        {
            var data = JsonConvert.DeserializeObject<SearchData>(data_json);
            var words = data.Query.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            if (data.TitleSearch)
            {
                var tres = SearchApi.SearchTitles(data.Query);

                var title_links = tres.Select(f => new WebMvc.Models.Result()
                {
                    IsMeta = true,
                    Description = GetWords(f.Fragment, words),
                    Title = GetWords(f.Title, words),
                    Url = "http://stackoverflow.com/questions/" + f.Id,
                    Id = f.Id
                })
                .ToList();

                data.Links = title_links;
            }
            else
            {
                var mres = SearchApi.SearchMain(data.Query, data.Which);
                var main_links = mres.SelectMany(z => z.Items, (p, c) => new WebMvc.Models.Result()
                {
                    IsMeta = false,
                    Description = GetWords(c.Fragment, words),
                    Title = GetWords(c.Title, words),
                    Score = c.Score,
                    Url = "http://stackoverflow.com/questions/" + c.Id,
                    Id = c.Id
                })
                //.Where(f => !title_links.Any(z => z.Id == f.Id))
                .ToList();
                //main_links.AddRange(title_links);
                data.Links = main_links;
            }
            return JsonConvert.SerializeObject(data);
        }

        List<WebMvc.Models.Word> GetWords(string text, List<string> words)
        {
            
            var twords = text.Split();
            var res = new List<WebMvc.Models.Word>();
            foreach (var w in twords)
            {
                res.Add(new WebMvc.Models.Word()
                {
                    IsBold = words.Any(f => w.ToLower().StartsWith(f.ToLower())),
                    Token = w
                });
            }
            return res;
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
        //private static string DecompressBody(HttpRequestBase request)
        //{
        //    string compr;
        //    using (Stream req = request.InputStream)
        //    {
        //        req.Seek(0, System.IO.SeekOrigin.Begin);
        //        using (var r = new StreamReader(req))
        //        {
        //            compr = r.ReadToEnd();
        //        }
        //    }
        //    compr = Utils.Util.Decompress(compr);

        //    return compr;
        //}
    }

    //global.asax Do().
    //public class LinkAcceptor
    //{
    //    public static object _lock = new object();
    //    public static List<string> Links = new List<string>();

    //    public static int thread_count = 0;
    //    public static object _count_lock = new object();
    //    static void IncrementCount()
    //    {
    //        lock (_count_lock)
    //        {
    //            thread_count++;
    //        }
    //    }
    //    static void DecrementCount()
    //    {
    //        lock (_count_lock)
    //        {
    //            thread_count--;
    //        }
    //    }

    //    public static void Do()
    //    {

    //        if (!MyTable.StatsCrawlerExists(Const.FailedCrawls))
    //        {
    //            MyTable.InsertStatsCrawler(new StatsEntity(Const.FailedCrawls, "0"));
    //        }
    //        if (!MyTable.StatsCrawlerExists(Const.TotalCrawls))
    //        {
    //            MyTable.InsertStatsCrawler(new StatsEntity(Const.TotalCrawls, "0"));
    //        }


    //        List<string> _links = new List<string>();
    //        object _links_lock = new object();
    //        List<CrawlResult> result = new List<CrawlResult>();
    //        object _result_lock = new object();
    //        while (true)
    //        {
    //            try
    //            {
    //                lock (_links_lock)
    //                {
    //                    if (!_links.Any())
    //                    {
    //                        lock (_lock)
    //                        {
    //                            _links.AddRange(Links);
    //                            Links = new List<string>();
    //                        }

    //                        if (!_links.Any())
    //                        {
    //                            Thread.Sleep(10000);
    //                            continue;
    //                        }
    //                    }
    //                }

    //                if (thread_count < Convert.ToInt32(ConfigurationManager.AppSettings["Concurrent_Threads"] + ""))
    //                {
    //                    IncrementCount();
    //                    System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(f =>
    //                    {
    //                        try
    //                        {
    //                            string link;
    //                            lock (_links_lock)
    //                            {
    //                                if (!_links.Any())
    //                                {
    //                                    return;
    //                                }

    //                                link = _links[0];
    //                                _links.RemoveAt(0);
    //                            }

    //                            if (Helpers.RobotsAllow(new Uri(link)).Result)
    //                            {
    //                                var r = CrawlerHelper.Crawl(link).Result;

    //                                if (r.IsSuccess)
    //                                {
    //                                    lock (_result_lock)
    //                                    {
    //                                        result.Add(r);
    //                                        if (result.Count() > 10)
    //                                        {
    //                                            InterMachine.MasterProceedResults(ConfigurationManager.AppSettings["Master"] + "", result);
    //                                            result = new List<CrawlResult>();
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            MyTable.LogInfo("Node crawling error ", ex, true);
    //                        }
    //                        finally
    //                        {
    //                            DecrementCount();
    //                        }
    //                    });
    //                }
    //                else
    //                {
    //                    Thread.Sleep(2000);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                MyTable.LogInfo("Node ERROR starting crawling threads ", ex, true);
    //            }
    //        }
    //    }
    //}
}