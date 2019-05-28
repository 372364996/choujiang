using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.Domains;
using Components.Helper;
using log4net;
using Newtonsoft.Json;

namespace Components.Providers
{
    public class WxProvider
    {
        protected static ILog logger = LogManager.GetLogger(typeof(WxProvider));

        #region 微信小程序
        /// <summary>
        /// 获取/绑定用户
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="sessionKey"></param>
        /// <param name="encryptedDataStr"></param>
        /// <param name="iv"></param>
        /// <returns>unionID</returns>
        //public string GetUser(string appid, string sessionKey, string encryptedDataStr, string iv)
        //{
        //    var encryptedData = WXBizDataCrypt.DecryptData(sessionKey, encryptedDataStr, iv);
        //    UserInfoFull userinfoFull = JsonConvert.DeserializeObject<UserInfoFull>(encryptedData);
        //    if (userinfoFull.watermark.appid != appid)
        //    {
        //        throw new Exception("userinfofull.wartemark.appid 不等于 appid!");
        //    }

        //   var user=db.LoadByOpenId(userinfoFull.openId);

        //    if (token == null)
        //    {
        //        user = UserService.Create(userinfoFull.nickName, userinfoFull.gender, userinfoFull.country, userinfoFull.province, userinfoFull.city);

        //        //获取用户头像
        //        string headImg = userinfoFull.avatarUrl;
        //        if (!String.IsNullOrEmpty(headImg))
        //        {
        //            //下载头像并保存
        //            string rootUrl = headImg.Substring(0, headImg.LastIndexOf("/"));
        //            string headImgHash = CryptoHelper.Md5(rootUrl);

        //            //下载原尺寸、64的两个
        //            int[] sizes = new int[] { 0, 64 };
        //            //WebClient webCLient = new WebClient();
        //            foreach (var size in sizes)
        //            {
        //                string hurl = rootUrl + "/" + size;
        //                try
        //                {
        //                    byte[] buffer = Utils.DownloadData(hurl);
        //                    string dest = String.Format("headimgs/{0}/{1}.png", user.Id, size);
        //                    StorageProvider.UploadFile(buffer, dest);
        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Error("下载用户头像失败：" + hurl, e);
        //                }
        //            }

        //            user.HeadImg = "headimgs/" + user.Id;
        //            user.HeadImgHash = headImgHash;
        //        }
        //        logger.DebugFormat("创建用户{0}成功", user.Name);


        //    }
        //    else
        //    {
        //        user = UserService.Load(token.UserId); //首先根据token获取user

        //        if (user.LastImgTime.AddDays(3) < DateTime.UtcNow)     //判断是否超过三天未更新用户基本信息
        //        {
        //            //更新用户信息
        //            UpdateUserInfo(userinfoFull, user);
        //        }
        //        //UserService.Update();   //将用户基本的信息和最后更新时间保存
        //    }
        //    token.ExpiredIn = userinfoFull.watermark.timestamp;
        //    token.UpdateTime = DateTime.UtcNow;

        //    WcRepository.SaveChanges();

        //    return userinfoFull.unionId;
        //}

        public class UserInfoFull
        {
            public string openId { get; set; }
            public string nickName { get; set; }
            public int gender { get; set; }
            public string language { get; set; }
            public string city { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string avatarUrl { get; set; }
            public string unionId { get; set; }
            public Watermark watermark { get; set; }
        }

        public class Watermark
        {
            public int timestamp { get; set; }
            public string appid { get; set; }
        }
        #endregion
    }
}
