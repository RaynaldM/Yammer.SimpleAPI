using System;
using System.Web.Mvc;
using Yammer.api.web.Models;

namespace Yammer.api.web.Controllers
{
    public class HomeController : Controller
    {
        private  String _yammerCode;
        private  String _yammerToken;
        private YammerClient _myYammer;
        protected YammerClient MyYammerClient
        {
            get
            {
                return this._myYammer ?? (this._myYammer = this._yammerToken != null
                                                               ? new YammerClient(this._yammerToken)
                                                               : new YammerClient("Your_clientID",
                                                                                  "Your_SecretKey",
                                                                                  Request.Url.AbsoluteUri +
                                                                                  Url.Action("AuthCode"),
                                                                                  this._yammerCode));
            }
        }
        //
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
                var url = this.MyYammerClient.GetLoginLinkUri();
                return Redirect(url);
            }
            return View(model);
        }

        public ActionResult AuthCode(String code)
        {
            if (!String.IsNullOrWhiteSpace(code))
            {
                this._yammerCode = code;
                this._yammerToken = this.MyYammerClient.GetToken();
                var l = this.MyYammerClient.GetUsers();
                //var tk= this.MyYammerClient.GetImpersonateTokens();
               // var i = this.MyYammerClient.SendInvitation("test@test.fr");
                //var m = this.MyYammerClient.PostMessage("A test from here", 0, "Event" topic);
                return View(this.MyYammerClient.GetUserInfo());
            }
            return null;
        }
    }
}
