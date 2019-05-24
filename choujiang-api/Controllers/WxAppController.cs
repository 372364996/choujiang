using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Components.Domains;
using Components.Helper;
using Components.Repositories.Ef;
using log4net;
using Newtonsoft.Json;

namespace choujiang_api.Controllers
{
    public class WxAppController : Controller
    {
        protected static ILog logger = LogManager.GetLogger(typeof(WxAppController));
        protected static string AppId = "wx74c6cc8e1fac314c";
        protected static string AppSecret = "a78f78cd1a44b076efe933b16e871af3";
        protected static string GetSessionKeyUrl = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
        protected ChouJiangDbContext db = new ChouJiangDbContext();
        private User user = null;

        protected User CurrentUser
        {
            get
            {
                if (user == null && !string.IsNullOrEmpty(Request["openid"]))
                {
                    try
                    {
                         user = db.Users.ToList().Find(u=>u.OpenId==Request["openid"].ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error("获取微信小程序用户登录信息失败:" + ex.Message);
                        return null;
                    }
                }

                return user;
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            string error = Utils.GetRandomString("0123456789", 6);
            Session["errorcode"] = error;
            var context = filterContext.HttpContext;
            string data = "";
            if (context.Request.Form != null && context.Request.Form.Count > 0)
            {
                data = JsonConvert.SerializeObject(context.Request.Form);
            }
            string msg = String.Format(@"{0}
URL:{1}
REFER:{2}
USER:{3}
DATA:{4}
{5}", error,
context.Request.Url.ToString(),
context.Request.UrlReferrer != null ? filterContext.HttpContext.Request.UrlReferrer.ToString() : "NULL",
context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "NOT AUTH",
data,
Utils.ExceptionToString(filterContext.Exception));
            logger.Error(msg);
            base.OnException(filterContext);
        }
    }
}