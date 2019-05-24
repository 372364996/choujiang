using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Components.Domains;
using Components.Helper;
using Newtonsoft.Json;

namespace choujiang_api.Controllers
{
    public class HomeController : WxAppController
    {
        public JsonResult Login(string encryptedData, string iv, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, errMsg = "获取code失败！" }, JsonRequestBehavior.AllowGet);
            }

            //string sessionKey;
            //if (!SessionKeys.TryGetValue(model.session_id, out sessionKey))
            //{
            //    return Json(new { success = false, errMsg = "在安全字典中获取session_key失败" });
            //}
            var url = string.Format(GetSessionKeyUrl, AppId, AppSecret, code);
            logger.Debug(url);
            var jsonStr = Encoding.UTF8.GetString(new WebClient().DownloadData(url));
            logger.Debug(jsonStr);
            var session = JsonConvert.DeserializeObject<SessionKey>(jsonStr);
            if (string.IsNullOrEmpty(session.session_key))
            {
                return Json(new { success = true, errMsg = "session_key参数:null" }, JsonRequestBehavior.AllowGet);
            }
               var user = db.Users.ToList().Find(u => u.OpenId == session.openid);
            try
            {
            
                if (user==null)
                {
                    user.OpenId = session.openid;
                    user.CreateTime=DateTime.Now;
                    user.Account= new Account()
                    {
                        Money = 0,
                        MoneyLocked = 0,
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                //session_id = CryptoHelper.Base64Encode(WxProvider.GetUser(AppId, session.session_key, encryptedData, iv));
            }
            catch (Exception ex)
            {
                logger.Error("save user:error," + ex.Message);
                return Json(new { success = false, errMsg = "save user:error," + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, errMsg = "save user:ok", openid = session.openid }, JsonRequestBehavior.AllowGet);
        }
    }

    public class SessionKey
    {
        public string openid { get; set; }
        public string session_key { get; set; }
    }
}