using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class JsonData
    {
        public string html { get; set; }
        public int total_steps { get; set; }
        public int page { get; set; }
        public string step_info { get; set; }
        public string meta_res { get; set; }
        public int count { get; set; }
    }
    public class HomeController : Controller
    {
        public static string RenderPartialViewToString(Controller thisController, string viewName, object model)
        {
            // assign the model of the controller from which this method was called to the instance of the passed controller (a new instance, by the way)
            thisController.ViewData.Model = model;

            // initialize a string builder
            using (StringWriter sw = new StringWriter())
            {
                // find and load the view or partial view, pass it through the controller factory
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(thisController.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(thisController.ControllerContext, viewResult.View, thisController.ViewData, thisController.TempData, sw);

                // render it
                viewResult.View.Render(viewContext, sw);

                //return the razorized view/partial-view as a string
                return sw.ToString();
            }
        }

        [ValidateInput(false)]
        public ActionResult Index(SearchData data)
        {
            return View(data);
        }

        [ValidateInput(false)]
        public ActionResult SearchPage(SearchData data)
        {
            if (string.IsNullOrEmpty(data.Query))
            {
                JsonData jd = new JsonData()
                {
                    html = "",
                    page = data.Page,
                    total_steps = data.TotalSteps,
                    step_info = data.StepInfo,
                    meta_res = data.MetaRes,
                    count = data.Links == null ? 0 : data.Links.Count()
                };
                return Json(jd, JsonRequestBehavior.AllowGet);
            }

            if (data.Query.Split().Count() > 1 && (data.Query + "").Length > 45)
            {
                data.Query = data.Query.Substring(0, 45);
                var ind = data.Query.LastIndexOf(" ");
                if (ind > 0)
                {
                    data.Query = data.Query.Substring(0, ind);
                }
            }
            var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
            var ns = nodes.Split(";".ToCharArray());
            var tasks = new List<Task<SearchData>>();
            data.TitleSearch = true;
            foreach (var n in ns)
            {
                var t = Task.Run<SearchData>(() =>
                    {
                        try
                        { 
                            var res = InterMachine.SearchNode(n, data);
                            return JsonConvert.DeserializeObject<SearchData>(res);
                        }
                        catch (Exception ex)
                        {
                            //MyTable.LogInfo(string.Format("Master ERROR searching node {0}: ", n), ex, true);
                            return null;
                        }
                    });
                tasks.Add(t);
            }

            var res_links = new List<WebMvc.Models.Result>();
            foreach (var t in tasks)
            {
                if (t.Result != null && t.Result.Links != null)
                {
                    res_links.AddRange(t.Result.Links);
                }
            }

            if (res_links.Count() < 10)
            {
                data.TitleSearch = false;
                data.Which = 0;
                foreach (var n in ns)
                {
                    if (n.Contains("7777"))
                    {
                        continue;
                    }
                    var t = Task.Run<SearchData>(() =>
                    {
                        try
                        {
                            var res = InterMachine.SearchNode(n, data);
                            return JsonConvert.DeserializeObject<SearchData>(res);
                        }
                        catch (Exception ex)
                        {
                            //MyTable.LogInfo(string.Format("Master ERROR searching node {0}: ", n), ex, true);
                            return null;
                        }
                    });
                    tasks.Add(t);
                }
                foreach (var t in tasks)
                {
                    if (t.Result != null && t.Result.Links != null)
                    {
                        res_links.AddRange(t.Result.Links);
                    }
                }
            }

            if (res_links.Count() < 10)
            {
                data.TitleSearch = false;
                data.Which = 0;
                foreach (var n in ns)
                {
                    if (n.Contains("7979"))
                    {
                        continue;
                    }
                    var t = Task.Run<SearchData>(() =>
                    {
                        try
                        {
                            var res = InterMachine.SearchNode(n, data);
                            return JsonConvert.DeserializeObject<SearchData>(res);
                        }
                        catch (Exception ex)
                        {
                            //MyTable.LogInfo(string.Format("Master ERROR searching node {0}: ", n), ex, true);
                            return null;
                        }
                    });
                    tasks.Add(t);
                }
                foreach (var t in tasks)
                {
                    if (t.Result != null && t.Result.Links != null)
                    {
                        res_links.AddRange(t.Result.Links);
                    }
                }
            }

            if (res_links.Count() < 10)
            {
                data.TitleSearch = false;
                data.Which = 1;
                foreach (var n in ns)
                {
                    if (n.Contains("7777"))
                    {
                        continue;
                    }
                    var t = Task.Run<SearchData>(() =>
                    {
                        try
                        {
                            var res = InterMachine.SearchNode(n, data);
                            return JsonConvert.DeserializeObject<SearchData>(res);
                        }
                        catch (Exception ex)
                        {
                            //MyTable.LogInfo(string.Format("Master ERROR searching node {0}: ", n), ex, true);
                            return null;
                        }
                    });
                    tasks.Add(t);
                }
                foreach (var t in tasks)
                {
                    if (t.Result != null && t.Result.Links != null)
                    {
                        res_links.AddRange(t.Result.Links);
                    }
                }
            }

            if (res_links.Count() < 10)
            {
                data.TitleSearch = false;
                data.Which = 1;
                foreach (var n in ns)
                {
                    if (n.Contains("7979"))
                    {
                        continue;
                    }
                    var t = Task.Run<SearchData>(() =>
                    {
                        try
                        {
                            var res = InterMachine.SearchNode(n, data);
                            return JsonConvert.DeserializeObject<SearchData>(res);
                        }
                        catch (Exception ex)
                        {
                            //MyTable.LogInfo(string.Format("Master ERROR searching node {0}: ", n), ex, true);
                            return null;
                        }
                    });
                    tasks.Add(t);
                }
                foreach (var t in tasks)
                {
                    if (t.Result != null && t.Result.Links != null)
                    {
                        res_links.AddRange(t.Result.Links);
                    }
                }
            }

            data.Links = res_links.Where(f => f.IsMeta).OrderByDescending(f => f.Score).Take(5).ToList();
            data.Links.AddRange(res_links.Where(f => !f.IsMeta && !data.Links.Any(z => z.Id == f.Id)).OrderByDescending(f => f.Score).Take(10 - data.Links.Count()));
            if (data.Links.Count() < 10)
            { 
                data.Links.AddRange(res_links.Where(f => f.IsMeta && !data.Links.Any(z => z.Id == f.Id)).OrderByDescending(f => f.Score).Take(10 - data.Links.Count()).ToList());
            }

            var result = RenderPartialViewToString(this, "SearchPage", data);

            JsonData jdr = new JsonData()
            {
                html = result,
                page = data.Page,
                total_steps = data.TotalSteps,
                step_info = data.StepInfo,
                meta_res = data.MetaRes,
                count = data.Links == null ? 0 : data.Links.Count()
            };
            return Json(jdr, JsonRequestBehavior.AllowGet);
        }

        //public string MetaIndex(string query, string synonyms, string freqs, string original_query)
        //{
        //    var data = SearchMeta.SearchTitles(query, synonyms, freqs, original_query);
        //    return JsonConvert.SerializeObject(data);
        //}

        //public string NodeIndex(string query, string phases)
        //{
        //    var data = SearchUtils.NodeSearch(query, phases);
        //    return JsonConvert.SerializeObject(data);
        //}

        public ActionResult About()
        {
            return View();
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

        //public ActionResult Slow()
        //{
        //    var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
        //    var ns = nodes.Split(";".ToCharArray());
        //    var tasks = new List<Task<List<QueryLogEntity>>>();
        //    foreach (var n in ns)
        //    {
        //        var t = Task.Run<List<QueryLogEntity>>(() =>
        //        {
        //            var res = InterMachine.GetNodesSlow(n);
        //            res = Utils.Util.Decompress(res);
        //            return JsonConvert.DeserializeObject<List<QueryLogEntity>>(res);
        //        });
        //        tasks.Add(t);
        //    }

        //    var res_links = new List<List<QueryLogEntity>>();
        //    foreach (var t in tasks)
        //    {
        //        t.Wait(10000);
        //        if (t.Result != null)
        //        {
        //            res_links.Add(t.Result);
        //        }
        //    }

        //    var mres = MyTable.GetQuerySlow();
        //    foreach (var r in mres)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    res_links.Add(mres);

        //    return View("Log", res_links.SelectMany(f => f).OrderByDescending(f => f.Date).Take(150).ToList());
        //}

        //public ActionResult Log()
        //{
        //    var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
        //    var ns = nodes.Split(";".ToCharArray());
        //    var tasks = new List<Task<List<QueryLogEntity>>>();
        //    foreach (var n in ns)
        //    {
        //        var t = Task.Run<List<QueryLogEntity>>(() =>
        //        {
        //            var res = InterMachine.GetNodesLog(n);
        //            res = Utils.Util.Decompress(res);
        //            return JsonConvert.DeserializeObject<List<QueryLogEntity>>(res);
        //        });
        //        tasks.Add(t);
        //    }

        //    var res_links = new List<List<QueryLogEntity>>();
        //    foreach (var t in tasks)
        //    {
        //        t.Wait(10000);
        //        if (t.Result != null)
        //        {
        //            res_links.Add(t.Result);
        //        }
        //    }

        //    var mres = MyTable.GetQueryLog(50);
        //    foreach (var r in mres)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    res_links.Add(mres);

        //    return View(res_links.SelectMany(f => f).OrderByDescending(f => f.Date).Take(150).ToList());
        //}

        //public ActionResult Errors()
        //{
        //    var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
        //    var ns = nodes.Split(";".ToCharArray());
        //    var tasks = new List<Task<List<LogEntity>>>();
        //    foreach (var n in ns)
        //    {
        //        var t = Task.Run<List<LogEntity>>(() =>
        //        {
        //            var res = InterMachine.GetNodesErrors(n);
        //            res = Utils.Util.Decompress(res);
        //            return JsonConvert.DeserializeObject<List<LogEntity>>(res);
        //        });
        //        tasks.Add(t);
        //    }

        //    var res_links = new List<List<LogEntity>>();
        //    foreach (var t in tasks)
        //    {
        //        t.Wait(10000);
        //        if (t.Result != null)
        //        {
        //            res_links.Add(t.Result);
        //        }
        //    }

        //    var mres = MyTable.GetErrors(50);
        //    foreach (var r in mres)
        //    {
        //        r.Node = ConfigurationManager.AppSettings["MyAddress"] + "";
        //    }
        //    res_links.Add(mres);

        //    return View(res_links.SelectMany(f => f).OrderByDescending(f => f.DateUtc).Take(150).ToList());
        //}

        //public ActionResult Stats()
        //{
        //    var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
        //    var ns = nodes.Split(";".ToCharArray());
        //    var tasks = new List<Task<string>>();
        //    foreach (var n in ns)
        //    {
        //        var t = Task.Run<string>(() =>
        //        {
        //            return InterMachine.GetNodesStats(n);
        //        });
        //        tasks.Add(t);
        //    }

        //    var res_links = new List<string>();
        //    foreach (var t in tasks)
        //    {
        //        t.Wait(10000);
        //        if (!string.IsNullOrEmpty(t.Result))
        //        {
        //            res_links.Add(t.Result);
        //        }
        //    }

        //    var total = MyTable.GetStatsCrawler(Const.TotalCrawls);
        //    var failed = MyTable.GetStatsCrawler(Const.FailedCrawls);
        //    res_links.Add((total != null ? total.Value : "0") + "|" + (failed != null ? failed.Value : "0"));

        //    var res = new StatsData();
        //    foreach (var r in res_links)
        //    {
        //        var parts = r.Split("|".ToCharArray());
        //        res.Total += Convert.ToInt32(parts[0]);
        //        res.Failed += Convert.ToInt32(parts[1]);
        //    }

        //    return View(res);
        //}

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //[HttpGet]
        //public ActionResult Synonyms()
        //{
        //    var syn = AzureStorage.AzureGetSynonyms();
        //    if (!syn.Any())
        //    {
        //        return View(new SynonymsData());
        //    }
        //    var data = syn.Select(f => f.Item1 + "=" + f.Item2)
        //                   .OrderBy(f => f)
        //                   .Aggregate((a, b) => a + Environment.NewLine + b);

        //    return View(new SynonymsData() { Value = data });
        //}
        //[HttpPost]
        //public ActionResult Synonyms(SynonymsData data)
        //{
        //    if (ConfigurationManager.AppSettings["IsNode"] == "0")
        //    { 
        //        var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
        //        var ns = nodes.Split(";".ToCharArray());
        //        var tasks = new List<Task>();
        //        foreach (var n in ns)
        //        {
        //            var t = Task.Run(() =>
        //            {
        //                InterMachine.NodePutSynonyms(n, data);
        //            });
        //            tasks.Add(t);
        //        }

        //        foreach (var t in tasks)
        //        {
        //            t.Wait();
        //        }
        //    }

        //    var lines = data.Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var line in lines)
        //    {
        //        var parts = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
        //        string key = "";
        //        foreach(var w in Utils.Util.GetDistinctWords(parts[0].ToLower()))
        //        {
        //            key += w.Key + " "; 
        //        }
        //        key = key.Trim();

        //        string val = "";
        //        foreach(var s in parts[1].Split(",".ToCharArray()))
        //        {
        //            string cv = "";
        //            foreach (var w in Utils.Util.GetDistinctWords(s.ToLower()))
        //            {
        //                cv += w.Key + " ";
        //            }
        //            val += cv.Trim()+",";
        //        }
        //        AzureStorage.AzurePutString(key, val.Trim(",".ToCharArray()), WhichDB.SynonymsDB);
        //    }
        //    var syn = AzureStorage.AzureGetSynonyms();
        //    if (!syn.Any())
        //    {
        //        return View(new SynonymsData());
        //    }
        //    var res = syn.Select(f => f.Item1 + "=" + f.Item2)
        //                  .OrderBy(f => f)
        //                  .Aggregate((a, b) => a + Environment.NewLine + b);

        //    return View(new SynonymsData() { Value = res });
        //}

        //[HttpGet]
        //public string GetValue(string key, string which)
        //{
        //    WhichDB e = (WhichDB)Enum.Parse(typeof(WhichDB), which);
        //    if (e == WhichDB.IndexDB)
        //    {
        //        var k = MyTable.GetKeyInfo(key);
        //        switch (k.Type)
        //        {
        //            case MyTable.IndexType.Index:
        //                return JsonConvert.SerializeObject(MyTable.GetSomeIndex(key));
        //            case MyTable.IndexType.Title:
        //                return JsonConvert.SerializeObject(MyTable.GetSomeIndex(key));
        //            case MyTable.IndexType.Description:
        //                return JsonConvert.SerializeObject(MyTable.GetSomeIndex(key));
        //            case MyTable.IndexType.Url:
        //                return JsonConvert.SerializeObject(MyTable.GetSomeIndex(key));
        //            case MyTable.IndexType.Link:
        //                return JsonConvert.SerializeObject(MyTable.GetLinkIndexPerm(key));
        //            default:
        //                return "";
        //        }
        //    }
        //    else
        //    {
        //        return AzureStorage.AzureGetString(key, e);
        //    }
        //}

    }

    public class StatsData
    {
        public int Total { get; set; }
        public int Failed { get; set; }
    }
    public class SynonymsData
    {
        public string Value { get; set; }
    }

    public class StorageData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}