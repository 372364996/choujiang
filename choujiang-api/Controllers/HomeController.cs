using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Components.Domains;
using Components.Helper;
using log4net;
using Newtonsoft.Json;

namespace choujiang_api.Controllers
{
    public class HomeController : WxAppController
    {
        protected static ILog logger = LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            logger.Debug("进入首页");
            return View();
        }

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
                var encryptedDataStr = WXBizDataCrypt.DecryptData(session.session_key, encryptedData, iv);
                Models.UserInfo userinfoFull = JsonConvert.DeserializeObject<Models.UserInfo>(encryptedDataStr);
                if (user == null)
                {
                    user = new User();
                    user.OpenId = session.openid;
                    user.CreateTime = DateTime.Now;
                    user.Account = new Account()
                    {
                        Money = 0,
                        MoneyLocked = 0,
                    };
                    db.Users.Add(user);

                }
                //获取用户头像
                string headImg = userinfoFull.avatarUrl;
                if (!String.IsNullOrEmpty(headImg))
                {
                    //下载头像并保存
                    string rootUrl = headImg.Substring(0, headImg.LastIndexOf("/"));
                    string headImgHash = CryptoHelper.Md5(rootUrl);

                    //下载原尺寸、64的两个
                    int[] sizes = new int[] { 0, 64 };
                    //WebClient webCLient = new WebClient();
                    foreach (var size in sizes)
                    {
                        string hurl = rootUrl + "/" + size;
                        try
                        {
                            byte[] buffer = Utils.DownloadData(hurl);
                            string dest = String.Format("{0}.png", size);
                            string headImageDir = Path.Combine(Server.MapPath("~/Upload/") + String.Format("headimgs/{0}/", user.Id));
                            //判断目录
                            if (!Directory.Exists(headImageDir))
                            {
                                Directory.CreateDirectory(headImageDir);
                            }
                            string headPath = headImageDir + dest;
                            //判断文件
                            if (System.IO.File.Exists(headPath))
                            {
                                System.IO.File.Delete(headPath);
                            }
                            Image image = Utils.BytToImg(buffer);
                            image.Save(headPath);
                        }
                        catch (Exception e)
                        {
                            logger.Error("下载用户头像失败：" + hurl, e);
                        }
                    }

                    user.HeadImg = "headimgs/" + user.Id;
                    user.HeadImgHash = headImgHash;
                }
                user.Name = userinfoFull.nickName;
                user.Sex = userinfoFull.gender;
                user.Country = userinfoFull.country;
                user.City = userinfoFull.city;
                user.Province = userinfoFull.province;
                db.SaveChanges();
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