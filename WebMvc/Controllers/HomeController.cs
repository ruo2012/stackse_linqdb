using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
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

        public static string CreateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        [ValidateInput(false)]
        public ActionResult SearchPage(SearchData data)
        {
            if (string.IsNullOrEmpty(data.Query))
            {
                JsonData jd = new JsonData()
                {
                    html = "",
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

            var recent_hash = CreateMD5Hash(Request.UserHostAddress + " " + data.Query);
            var dic = MvcApplication._recent_data;
            if (dic.ContainsKey(recent_hash) && (DateTime.Now - dic[recent_hash]).TotalMilliseconds < 600)
            {
                dic[recent_hash] = DateTime.Now;
                JsonData jd = new JsonData()
                {
                    html = "Too many requests...",
                    meta_res = data.MetaRes,
                    count = data.Links == null ? 0 : data.Links.Count()
                };
                return Json(jd, JsonRequestBehavior.AllowGet);
            }
            dic[recent_hash] = DateTime.Now;

            var res_links = new List<WebMvc.Models.Result>();

            //titles
            Stopwatch titles_sw = new Stopwatch();
            titles_sw.Start();
            var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
            var ns = nodes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var data_copy = new SearchData()
            {
                TitleSearch = true,
                Query = data.Query
            };
            var tres = new List<SearchData>();
            var _tlock = new object();
            Parallel.ForEach(ns, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, n =>
            {
                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var res = InterMachine.SearchNode(n, data_copy);
                    sw.Stop();
                    var d = JsonConvert.DeserializeObject<SearchData>(res);
                    d.TimeInMs = sw.ElapsedMilliseconds;
                    d.Node = n;
                    lock (_tlock)
                    {
                        tres.Add(d);
                    }
                }
                catch (Exception ex)
                {
                    HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogError("Searching titles (" + data_copy.Query + ") " + n, ex));
                }
            });
            var log_datas = new List<SearchData>();
            foreach (var t in tres)
            {
                if (t != null)
                {
                    log_datas.Add(t);
                }
                if (t != null && t.Links != null)
                {
                    res_links.AddRange(t.Links);
                }
            }
            titles_sw.Stop();
            HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogQuery(log_datas, data_copy.Query, "Titles (" + (titles_sw.ElapsedMilliseconds) +" ms)"));

            //main
            if (res_links.Count() < 10)
            {
                Stopwatch main_sw = new Stopwatch();
                main_sw.Start();
                data_copy = new SearchData()
                {
                    TitleSearch = false,
                    Query = data.Query
                };
                tres = new List<SearchData>();
                var sw2 = new Stopwatch();
                sw2.Start();
                Parallel.ForEach(ns, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, n =>
                {
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        var res = InterMachine.SearchNode(n, data_copy);
                        sw.Stop();
                        var d = JsonConvert.DeserializeObject<SearchData>(res);
                        d.TimeInMs = sw.ElapsedMilliseconds;
                        d.Node = n;
                        lock (_tlock)
                        {
                            tres.Add(d);
                        }
                    }
                    catch (Exception ex)
                    {
                        HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogError("Searching main (" + data_copy.Query + ") " + n, ex));
                    }
                });
                sw2.Stop();

                log_datas = new List<SearchData>();
                foreach (var t in tres)
                {
                    if (t != null)
                    {
                        log_datas.Add(t);
                    }
                    if (t != null && t.Links != null)
                    {
                        res_links.AddRange(t.Links);
                    }
                }

                main_sw.Stop();
                HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogQuery(log_datas, data_copy.Query, "Main (" + (main_sw.ElapsedMilliseconds) + "ms, parallel " + sw2.ElapsedMilliseconds + " ms)"));
                //HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogError("Searching additional time: " + sw1.ElapsedMilliseconds + " ms", new Exception("")));
            }


            data.Links = res_links.Where(f => f.IsMeta).OrderByDescending(f => f.Score).Take(10).ToList();
            if (data.Links.Count() < 10)
            {
                data.Links.AddRange(res_links.Where(f => !f.IsMeta && !data.Links.Any(z => z.Id == f.Id)).OrderByDescending(f => f.Score).Take(10 - data.Links.Count()).ToList());
            }


            var result = RenderPartialViewToString(this, "SearchPage", data);


            

            JsonData jdr = new JsonData()
            {
                html = result,
                meta_res = data.MetaRes,
                count = data.Links == null ? 0 : data.Links.Count()
            };
            return Json(jdr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Log()
        {
            return View(MvcApplication.GetLogEvents());
        }
        public ActionResult Hello()
        {
            return View(MvcApplication.GetLogEventsHello());
        }
    }
}