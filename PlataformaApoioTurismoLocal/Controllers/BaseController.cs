using System;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    public class BaseController : Controller
    {
        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            HttpCookie cookie = new HttpCookie("_culture", lang)
            {
                Expires = DateTime.Now.AddYears(1)
            };
            Response.Cookies.Add(cookie);
            return Redirect(returnUrl);
        }
    }
}