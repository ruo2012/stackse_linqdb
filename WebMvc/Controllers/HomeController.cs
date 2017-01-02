using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
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

            object _lock = new object();
            var res_links = new List<WebMvc.Models.Result>();

            var title_search = Task.Run(() =>
            {
                Stopwatch titles_sw = new Stopwatch();
                titles_sw.Start();
                var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
                var ns = nodes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var tasks = new List<Task<SearchData>>();
                var data_copy = new SearchData()
                {
                    TitleSearch = true,
                    Query = data.Query
                };
                foreach (var n in ns)
                {
                    var t = Task.Run<SearchData>(() =>
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
                                return d;
                            }
                            catch (Exception ex)
                            {
                                HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogError("Searching titles (" + data_copy.Query + ") " + n, ex));
                                return null;
                            }
                        });
                    tasks.Add(t);
                }
                var log_datas = new List<SearchData>();
                foreach (var t in tasks)
                {
                    if (t.Result != null)
                    {
                        log_datas.Add(t.Result);
                    }
                    if (t.Result != null && t.Result.Links != null)
                    {
                        lock (_lock)
                        {
                            res_links.AddRange(t.Result.Links);
                        }
                    }
                }
                titles_sw.Stop();
                HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogQuery(log_datas, data_copy.Query, "Titles (" + (titles_sw.ElapsedMilliseconds) +" ms)"));
            });

            var main_search = Task.Run(() =>
            {
                Stopwatch main_sw = new Stopwatch();
                main_sw.Start();
                var nodes = ConfigurationManager.AppSettings["Nodes"] + "";
                var ns = nodes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var tasks = new List<Task<SearchData>>();
                var data_copy = new SearchData()
                {
                    TitleSearch = false,
                    Query = data.Query
                };
                foreach (var n in ns)
                {
                    var t = Task.Run<SearchData>(() =>
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
                            return d;
                        }
                        catch (Exception ex)
                        {
                            HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogError("Searching main (" + data_copy.Query + ") " + n, ex));
                            return null;
                        }
                    });
                    tasks.Add(t);
                }
                var log_datas = new List<SearchData>();
                foreach (var t in tasks)
                {
                    if (t.Result != null)
                    {
                        log_datas.Add(t.Result);
                    }
                    if (t.Result != null && t.Result.Links != null)
                    {
                        lock (_lock)
                        {
                            res_links.AddRange(t.Result.Links);
                        }
                    }
                }
                main_sw.Stop();
                HostingEnvironment.QueueBackgroundWorkItem(f => MvcApplication.LogQuery(log_datas, data_copy.Query, "Main (" + (main_sw.ElapsedMilliseconds) + " ms)"));
            });

            title_search.Wait();
            main_search.Wait();

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