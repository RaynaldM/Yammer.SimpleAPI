using System;
using System.Web.Mvc;
using Yammer.api.web.Models;

namespace Yammer.api.web.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/
        public ActionResult Index()
        {
            var model = new IndexViewModel
            {
                ClientId = "Your_clientID",
                ClientSecret = "Your_SecretKey"
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(IndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                var myConfig = new ClientConfigurationContainer
                                   {
                                                              ClientCode = null,
                                                              ClientId = model.ClientId,
                                                              ClientSecret = model.ClientSecret,
                                                              RedirectUri = Request.Url.AbsoluteUri + Url.Action("AuthCode")
                                                          };
                var myYammer = new YammerClient(myConfig);
                var url = myYammer.GetLoginLinkUri();
                this.TempData["YammerConfig"] = myConfig;
                return Redirect(url);
            }
            return View(model);
        }

        public ActionResult AuthCode(String code)
        {
            if (!String.IsNullOrWhiteSpace(code))
            {
                var myConfig = this.TempData["YammerConfig"] as ClientConfigurationContainer;
                myConfig.ClientCode = code;
                var myYammer = new YammerClient(myConfig);
                // var yammerToken = myYammer.GetToken();
                // var l = myYammer.GetUsers();
                // var t= myYammer.GetImpersonateTokens();
                // var i = myYammer.SendInvitation("test@test.fr");
                // var m = myYammer.PostMessage("A test from here", 0, "Event" topic);
                return View(myYammer.GetUserInfo());
            }
            return null;
        }
    }
}
