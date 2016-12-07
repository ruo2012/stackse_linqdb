using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebMvc.Controllers
{
    //public class MasterController : Controller
    //{

    //    static Dictionary<string, List<Link>> LinkData = new Dictionary<string, List<Link>>();
    //    static int LinkCounter = 0;
    //    static object _link_lock = new object();
    //    static void ProceedLinkResults(List<Link> data)
    //    {
    //        foreach (var l in data)
    //        {
    //            string hash = Util.CalculateMD5Hash(l.Href);
    //            var node_url = AzureStorage.AzureGetString(":dn:" + hash, WhichDB.Crawler);
    //            if (!string.IsNullOrEmpty(node_url))
    //            {
    //                lock (_link_lock)
    //                {
    //                    if (LinkData.ContainsKey(node_url))
    //                    {
    //                        LinkData[node_url].Add(l);
    //                    }
    //                    else
    //                    {
    //                        LinkData[node_url] = new List<Link>() { l };
    //                    }

    //                    LinkCounter++;
    //                }
    //            }
    //        }
    //        var LinkDataCopy = new Dictionary<string, List<Link>>();
    //        if (LinkCounter > 1000)
    //        {
    //            lock (_link_lock)
    //            {
    //                LinkDataCopy = LinkData;
    //                LinkData = new Dictionary<string, List<Link>>();
    //                LinkCounter = 0;
    //            }
    //            foreach (var k in LinkDataCopy)
    //            {
    //                InterMachine.IndexLinks(k.Key, k.Value);
    //            }

    //            LinkDataCopy = new Dictionary<string, List<Link>>();
    //        }
    //    }

    //    static object _results_lock = new object();
    //    [HttpPost]
    //    public void MasterProceedResults()
    //    {
    //        var body = DecompressBody(Request);
    //        List<CrawlResult> results = JsonConvert.DeserializeObject<List<CrawlResult>>(body);

    //        ThreadPool.QueueUserWorkItem(z =>
    //        {
    //            foreach (var cr in results)
    //            {
    //                try
    //                {
    //                    if (cr.IsSuccess)
    //                    {
    //                        lock (_results_lock)
    //                        {
    //                            long last = Convert.ToInt64(MyTable.GetStatsCrawler(Const.CrawlLast).Value);
    //                            last++;
    //                            var crawling = new CrawlingEntity(row_key: last.ToString(), Url: cr.SourceUrl, urls: cr.Links.Select(f => new CrawlingLink() { Link = f.Href.ToLower(), Text = f.Text }).ToList(), SiteKey: cr.SiteKey);
    //                            MyTable.InsertCrawling(crawling);
    //                            var stats = new StatsEntity(row_key: Const.CrawlLast, value: last.ToString());
    //                            MyTable.InsertStatsCrawler(stats);
    //                        }

    //                        ProceedLinkResults(cr.Links);
    //                    }
    //                    else
    //                    {
    //                        string hash = Util.CalculateMD5Hash(cr.SourceUrl);
    //                        AzureStorage.AzurePutString(":d:" + hash, null, WhichDB.Crawler);
    //                        AzureStorage.AzurePutString(":dn:" + hash, null, WhichDB.Crawler);
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    MyTable.LogInfo("Master ERROR proceeding results: ", ex, true);
    //                }
    //            }
    //        });
    //    }

    //    private static string DecompressBody(HttpRequestBase request)
    //    {
    //        string compr;
    //        using (Stream req = request.InputStream)
    //        {
    //            req.Seek(0, System.IO.SeekOrigin.Begin);
    //            using (var r = new StreamReader(req))
    //            {
    //                compr = r.ReadToEnd();
    //            }
    //        }
    //        compr = Utils.Util.Decompress(compr);

    //        return compr;
    //    }
    //}

    ////global.asax Do().
    //public class CrawlingStarter
    //{
    //    public static string GetNextNode()
    //    {
    //        var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
    //        var ns = nodes.Split(";".ToCharArray());
    //        while (true)
    //        {
    //            var rnd = new Random();
    //            foreach (var n in ns.OrderBy(item => rnd.Next()))
    //            {
    //                try
    //                {
    //                    var count = InterMachine.GetNodesLinksCount(n);
    //                    if (count < 1000)
    //                    {
    //                        return n;
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    MyTable.LogInfo(string.Format("Master ERROR trying to get next node {0}: ", n), ex, true);
    //                }
    //            }
    //            Thread.Sleep(10000);
    //        }
    //    }
    //    public static void Do()
    //    {
    //        if (!MyTable.StatsCrawlerExists(Const.CrawlLast))
    //        {
    //            MyTable.InsertStatsCrawler(new StatsEntity(Const.CrawlLast, "0"));
    //        }
    //        if (!MyTable.StatsCrawlerExists(Const.CrawlNext))
    //        {
    //            MyTable.InsertStatsCrawler(new StatsEntity(Const.CrawlNext, "0"));
    //        }
            
    //        var root = new List<string>();
    //        root.Add("http://stackoverflow.com/");
    //        root.Add("http://stackoverflow.com/users/");
    //        root.Add("http://serverfault.com/");
    //        root.Add("http://serverfault.com/users/");
    //        root.Add("http://superuser.com/");
    //        root.Add("http://superuser.com/users/");
    //        root.Add("http://askubuntu.com/");
    //        root.Add("http://askubuntu.com/users/");
    //        root.Add("http://unix.stackexchange.com/");
    //        root.Add("http://unix.stackexchange.com/users/");
    //        root.Add("http://programmers.stackexchange.com/");
    //        root.Add("http://programmers.stackexchange.com/users/");
    //        root.Add("http://dba.stackexchange.com/");
    //        root.Add("http://dba.stackexchange.com/users");
    //        root.Add("http://cstheory.stackexchange.com");
    //        root.Add("http://cs.stackexchange.com");
    //        root.Add("http://security.stackexchange.com/");
    //        root.Add("http://security.stackexchange.com/users");

    //        for (int i = 0; i < root.Count; i++)
    //        {
    //            root[i] = root[i].ToLower().Trim("/".ToCharArray());
    //        }

    //        var cr = new CrawlingEntity(row_key: "0", Url: "http://www.stackse.com", urls: root.Select(f => new CrawlingLink() { Link = f }).ToList(), SiteKey: 0);
    //        MyTable.InsertCrawling(cr);


    //        while (true)
    //        {
    //            try
    //            {
    //                var next = MyTable.GetStatsCrawler(Const.CrawlNext);
    //                long next_to_crawl = Convert.ToInt64(next.Value);
    //                var entity = MyTable.GetCrawling(next_to_crawl.ToString());
    //                if (entity == null)
    //                {
    //                    //Console.WriteLine("No crawling entity... {0}", next_to_crawl);
    //                    MyTable.LogInfo(string.Format("No crawling entity... {0}", next_to_crawl), null, true);
    //                    Thread.Sleep(10000);
    //                    continue;
    //                }

    //                if (entity.Urls != null)
    //                {
    //                    var links = new List<string>();

    //                    string node_url = GetNextNode();

    //                    foreach (var u in entity.Urls)
    //                    {
    //                        string url = u.Link;
    //                        Uri uri = new Uri(url);
    //                        string hash = Util.CalculateMD5Hash(url);
    //                        var status = AzureStorage.AzureGetString(":d:" + hash, WhichDB.Crawler);
    //                        if (status == null)
    //                        {
    //                            links.Add(u.Link);
    //                            AzureStorage.AzurePutString(":d:" + hash, DateTime.Now.ToString(CultureInfo.InvariantCulture), WhichDB.Crawler);
    //                            AzureStorage.AzurePutString(":dn:" + hash, node_url, WhichDB.Crawler);
    //                        }
    //                    }

    //                    InterMachine.NodeIndexThese(node_url, links);
    //                }

    //                next_to_crawl++;
    //                next.Value = next_to_crawl.ToString();
    //                MyTable.InsertStatsCrawler(next);
    //            }
    //            catch (Exception ex)
    //            {
    //                MyTable.LogInfo("Master crawling loop error: ", ex, true);
    //            }
    //        }
    //    }
    //    public class LinkData
    //    {
    //        public string Site_key { get; set; }
    //        public string Url { get; set; }
    //        public string Text { get; set; }
    //        public int ReferingSite { get; set; }
    //    }
    //}
}