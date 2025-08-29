using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFim.Filters
{
    public class CultureActionFilter : IActionFilter
    {
        private static readonly string[] Allowed = new[] { "pt", "en", "es", "fr"};

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var routeLang = (string)filterContext.RouteData.Values["lang"];

            var cookie = filterContext.HttpContext.Request.Cookies["_culture"];
            var cookieLang = cookie?.Value;

            var lang = !string.IsNullOrWhiteSpace(routeLang) ? routeLang
                     : !string.IsNullOrWhiteSpace(cookieLang) ? cookieLang
                     : "pt";

            lang = lang.ToLowerInvariant();
            if (!Allowed.Contains(lang)) lang = "pt";

            var ci = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            if (!string.IsNullOrWhiteSpace(routeLang) && cookieLang != lang)
            {
                var newCookie = new HttpCookie("_culture", lang)
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddYears(1)
                };
                filterContext.HttpContext.Response.Cookies.Add(newCookie);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
