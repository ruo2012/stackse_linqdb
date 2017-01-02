using LinqDb;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMvc.Controllers;
using WebMvc.Models;

namespace WebMvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static Db logdb { get; set; }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ServicePointManager.DefaultConnectionLimit = 500;

            if (ConfigurationManager.AppSettings["IsNode"] == "1")
            {
                SearchApi.Init();
            }
            else
            {
                string log_db_path = @"C:\Users\Administrator\Documents\stackoverflow\LOG";
                logdb = new Db(log_db_path);
            }
        }


        protected void Application_Error()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext.Error != null)
            {
                HostingEnvironment.QueueBackgroundWorkItem(f => LogError("Generic error", httpContext.Error));
            }
        }

        public static void LogError(string title, Exception ex)
        {
            var log_event = new LogEvent()
            {
                ShortInfo = title + " " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                MoreInfo = (ex.InnerException != null ? ex.InnerException.StackTrace : ex.StackTrace),
                Time = DateTime.Now,
                Type = 2
            };
            LogInfo(log_event);
        }

        public static void LogQuery(List<SearchData> res, string query, string type)
        {
            int log_type = query == "hello world" ? 3 : 1;
            string title = type + ": " + query;
            string details = "Success: " + res.Count(f => f != null && f.Links != null) + " ";
            foreach (var r in res)
            {
                details += "(" + r.Node.Substring(7, 10)  + " " + r.TimeInMs + "(" + r.NodeTimeInMs + ") ms " + (r.Links != null ? r.Links.Count() : 0) + ") ";
            }
            var log_event = new LogEvent()
            {
                ShortInfo = title,
                MoreInfo = details,
                Time = DateTime.Now,
                Type = log_type
            };
            LogInfo(log_event);
        }

        protected void Application_End()
        {
            if (ConfigurationManager.AppSettings["IsNode"] == "1")
            {
                SearchApi.Dispose();
            }
            else
            {
                logdb.Dispose();
            }
        }

        static public void LogInfo(LogEvent le)
        {
            logdb.Table<LogEvent>().Save(le);
        }

        static public List<LogEvent> GetLogEvents(int how_many = 200)
        {
            return logdb.Table<LogEvent>().Where(f => f.Type != 3).OrderByDescending(f => f.Time).Take(how_many).SelectEntity();
        }
        static public List<LogEvent> GetLogEventsHello(int how_many = 200)
        {
            return logdb.Table<LogEvent>().Where(f => f.Type == 3).OrderByDescending(f => f.Time).Take(how_many).SelectEntity();
        }
    }
}
