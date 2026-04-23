using System;
using System.Globalization;   
using System.Threading;       
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ProjetoFim
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            var ctx = HttpContext.Current;

            var rd = RouteTable.Routes.GetRouteData(new HttpContextWrapper(ctx));
            var lang = rd != null ? rd.Values["lang"] as string : null;

            if (string.IsNullOrWhiteSpace(lang))
                lang = ctx.Request["lang"];

            if (string.IsNullOrWhiteSpace(lang))
                lang = "pt"; 

            var culture = CultureInfo.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
