using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMvc.Controllers;

namespace WebMvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (ConfigurationManager.AppSettings["IsNode"] == "1")
            {
                SearchApi.Init();
            }
        }


        protected void Application_Error()
        {
            HttpContext httpContext = HttpContext.Current;
        }

        protected void Application_End()
        {
            if (ConfigurationManager.AppSettings["IsNode"] == "1")
            {
                SearchApi.Dispose();
            }
        }
    }
}
