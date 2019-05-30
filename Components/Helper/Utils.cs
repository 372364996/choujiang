using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Net.Mail;

namespace Components.Helper
{
    public class Utils
    {
        protected static ILog logger = LogManager.GetLogger(typeof(Utils));
        static TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

        private static string storageUrl = "";
        private static string hostUrl = "";

        public static DateTime ToUtcTime(DateTime local)
        {
            return TimeZoneInfo.ConvertTimeToUtc(local, tzi);
        }

        public static DateTime ToLocalTime(DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
        }

        public static DateTime ParseUtcTime(string utcTimeString)
        {
            return DateTime.Parse(utcTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }
        public static string ExceptionToString(Exception exp)
        {
            if (exp == null)
            {
                return "";
            }
            string msg = exp.ToString();
            if (exp.InnerException != null)
            {
                msg += "\r\n" + ExceptionToString(exp.InnerException);
            }

            return msg;
        }

        public static DateTime MinDateTime { get { return DateTime.Parse("1970-1-1"); } }
        public static DateTime MaxDateTime { get { return DateTime.Parse("2049-1-1"); } }

        public static string UploadData(string url, string postData)
        {
            using (WebClient client = new WebClient())
            {
                return Encoding.UTF8.GetString(client.UploadData(url, Encoding.UTF8.GetBytes(postData)));
            }
        }
        public static string UploadFile(string url, string filepath)
        {
            using (WebClient client = new WebClient())
            {
                return Encoding.UTF8.GetString(client.UploadFile(url, filepath));
            }
        }
        public static string GetData(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(url);
            }
        }
        public static string GetDataForLongTime(string url)
        {
            using (NewWebClient client = new NewWebClient(30 * 60 * 1000))
            {
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(url);
            }
        }
        public static byte[] DownloadData(String url)
        {
            try
            {
                Byte[] bytes;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.KeepAlive = false;
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.ServicePoint.ConnectionLimit = Environment.ProcessorCount * 12;
                webRequest.Headers.Add("UserAgent", "Pentia; MSI");
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    String contentType = webResponse.ContentType;
                    using (Stream stream = webResponse.GetResponseStream())
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            // Stream the response to a MemoryStream via the byte array buffer
                            Byte[] buffer = new Byte[0x1000];

                            Int32 bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, bytesRead);
                            }
                            bytes = memoryStream.ToArray();
                        }
                    }
                }
                return bytes;
            }
            catch (Exception ex)
            {
                logger.Error("从" + url + "下载内容失败", ex);
                throw;
            }
        }
        //public static byte[] DownloadData(string url)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    byte[] ret = null;

        //    try
        //    {
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                var stream = response.GetResponseStream();
        //                byte[] buffer = new byte[10240];
        //                long length = response.ContentLength;
        //                long readed = 0;
        //                while (readed < length)
        //                {
        //                    int r = stream.Read(buffer, 0, buffer.Length);
        //                    if (r > 0)
        //                    {
        //                        ms.Write(buffer, 0, r);
        //                        readed += r;
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }

        //                ms.Seek(0, SeekOrigin.Begin);
        //                ret = ms.ToArray();
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("下载文件失败，" + response.StatusCode);
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        logger.Error("下载文件失败", exp);
        //        if (ret != null)
        //        {
        //            logger.Debug(Encoding.UTF8.GetString(ret));
        //        }
        //        throw;
        //    }
        //    finally
        //    {
        //        response.Close();
        //    }
        //    return ret;
        //}

        internal static string GenerateBillingNumber()
        {
            return "BN" + Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss") + GetRandomString("0123456789", 6);
        }

        public static byte[] DownloadFile(string url, out string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            byte[] ret = null;

            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var stream = response.GetResponseStream();
                        byte[] buffer = new byte[10240];
                        long length = response.ContentLength;
                        long readed = 0;
                        while (readed < length)
                        {
                            int r = stream.Read(buffer, 0, buffer.Length);
                            if (r > 0)
                            {
                                ms.Write(buffer, 0, r);
                                readed += r;
                            }
                            else
                            {
                                break;
                            }
                        }

                        ms.Seek(0, SeekOrigin.Begin);
                        ret = ms.ToArray();
                    }

                    string strFn = response.GetResponseHeader("Content-Disposition");
                    int start = strFn.IndexOf("\"") + 1;
                    int end = strFn.LastIndexOf("\"");
                    fileName = strFn.Substring(start, end - start);
                }
                else
                {
                    throw new Exception("下载文件失败，" + response.StatusCode);
                }
            }
            catch (Exception exp)
            {
                logger.Error("下载文件失败", exp);
                if (ret != null)
                {
                    logger.Debug(Encoding.UTF8.GetString(ret));
                }
                throw;
            }
            finally
            {
                response.Close();
            }
            return ret;
        }

        public static void CalcPager(int index, int size, out int from, out int to)
        {
            from = (index - 1) * size + 1;
            to = index * size;
        }
        private static Random random = new Random();

        public static string GetRandomString()
        {
            return GetRandomString("abcdefghijklmnopqrstuvwxyz0123456789", 8);
        }

        public static string GetOrderNumber()
        {
            return "CO" + Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss") + GetRandomString("0123456789", 6);
        }

        public static string GetChargeNumber(int userId)
        {
            return "CZ" + Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss") + GetRandomString("0123456789", 6) + "U" + userId;
        }

        public static string GetPayNumber()
        {
            return "ZF" + Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss") + GetRandomString("0123456789", 6);
        }

        public static string GetBonusNumber(int clsId, int userId)
        {
            return String.Format("CB{0}Z{1}Z{2}", clsId, userId, Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss"));
        }

        /// <summary>
        /// CBZ{ 类型 }Z{ 课程ID }Z{ 用户ID }Z{ 数量 }Z{ 时间唯一字符串 }
        /// </summary>
        /// <param name="clsId"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string GetBonusNumberByType(int type, int clsId, int userId, int num)
        {

            //商户系统内部订单号，要求32个字符内，只能是数字、大小写字母_-|* 且在同一个商户号下唯一。详见商户订单号
            if (type == 0)
            {
                //打赏
                // return $"CB{type}Z{clsId}Z{userId}Z{0}Z{Utils.ToLocalTime(DateTime.UtcNow):yyyyMMddHHmmss}";
                return $"CB{type}Z{clsId}Z{userId}Z{0}Z{ GetRandomString("0123456789", 6)}";

            }
            //礼物
            return
                $"CB{type}Z{clsId}Z{userId}Z{num}Z{GetRandomString("0123456789", 6)}";
        }

        public static string GetClassGroupNumber(int clsId, int userId)
        {
            return String.Format("CG{0}Z{1}Z{2}", clsId, userId, Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss"));
        }
        public static string GetCashOutNumber()
        {
            return "TX" + Utils.ToLocalTime(DateTime.UtcNow).ToString("yyyyMMddHHmmss") + GetRandomString("0123456789", 6);
        }
        public static string GetRandomString(string range, int length)
        {
            int strlen = range.Length;
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += range[random.Next(0, strlen)].ToString();
            }

            return str;
        }
        public static string GenerateNumber()
        {
            var dt = Utils.ToLocalTime(DateTime.UtcNow);
            return dt.ToString("yyyyMMddHHmmssttt") + GetRandomString("0123456789", 6);
        }

        public static byte[] GenThumbnail(byte[] srcImage, int width, int height)
        {
            int rw, rh;
            return GenThumbnail(srcImage, width, height, out rw, out rh);
        }
        public static byte[] GenThumbnail(byte[] srcImage, int width, int height, out int realWidth, out int realHeight)
        {
            Image imageFrom = null;
            using (MemoryStream ms = new MemoryStream(srcImage))
            {
                ms.Seek(0, SeekOrigin.Begin);
                imageFrom = Image.FromStream(ms);
            }
            // 源图宽度及高度 
            int imageFromWidth = imageFrom.Width;
            int imageFromHeight = imageFrom.Height;

            if (imageFromWidth <= width && imageFromHeight <= height)
            {
                realHeight = imageFromHeight;
                realWidth = imageFromWidth;
                return srcImage;
            }
            // 生成的缩略图实际宽度及高度 
            int bitmapWidth = width;
            int bitmapHeight = height;
            // 生成的缩略图在上述"画布"上的位置 
            int X = 0;
            int Y = 0;
            // 根据源图及欲生成的缩略图尺寸,计算缩略图的实际尺寸及其在"画布"上的位置 
            if (bitmapHeight * imageFromWidth > bitmapWidth * imageFromHeight)
            {
                bitmapHeight = imageFromHeight * width / imageFromWidth;
                //Y = (height - bitmapHeight) / 2;
            }
            else
            {
                bitmapWidth = imageFromWidth * height / imageFromHeight;
                //X = (width - bitmapWidth) / 2;
            }

            realWidth = bitmapWidth;
            realHeight = bitmapHeight;
            // 创建画布
            Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight);
            Graphics g = Graphics.FromImage(bmp);
            // 用白色清空 
            g.Clear(Color.White);
            // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // 指定高质量、低速度呈现。 
            g.SmoothingMode = SmoothingMode.HighQuality;
            // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
            g.DrawImage(imageFrom, new Rectangle(X, Y, bitmapWidth, bitmapHeight), new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //经测试 .jpg 格式缩略图大小与质量等最优 
                    bmp.Save(ms, ImageFormat.Jpeg);
                    ms.Seek(0, SeekOrigin.Begin);

                    return ms.ToArray();
                }
            }
            finally
            {
                //显示释放资源 
                bmp.Dispose();
                g.Dispose();
            }
        }

        public static byte[] GetHeadIocnThumbnail(byte[] srcImage, int width, int height)
        {
            Image imageFrom = null;

            using (MemoryStream ms = new MemoryStream(srcImage))
            {
                ms.Seek(0, SeekOrigin.Begin);
                imageFrom = Image.FromStream(ms);
            }
            //模版的宽高比例
            double templateRate = (double)width / height;
            //原图片的宽高比例
            double initRate = (double)imageFrom.Width / imageFrom.Height;
            // 生成的缩略图实际宽度及高度 
            int bitmapWidth = width;
            int bitmapHeight = height;
            //原图与模版比例相等，直接缩放
            if (templateRate == initRate)
            {

                // 源图宽度及高度 
                int imageFromWidth = imageFrom.Width;
                int imageFromHeight = imageFrom.Height;

                if (imageFromWidth <= width && imageFromHeight <= height)
                {
                    return srcImage;
                }

                // 生成的缩略图在上述"画布"上的位置 
                int X = 0;
                int Y = 0;
                // 根据源图及欲生成的缩略图尺寸,计算缩略图的实际尺寸及其在"画布"上的位置 
                if (bitmapHeight * imageFromWidth > bitmapWidth * imageFromHeight)
                {
                    bitmapHeight = imageFromHeight * width / imageFromWidth;
                    //Y = (height - bitmapHeight) / 2;
                }
                else
                {
                    bitmapWidth = imageFromWidth * height / imageFromHeight;
                    //X = (width - bitmapWidth) / 2;
                }
                // 创建画布 
                Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight);
                Graphics g = Graphics.FromImage(bmp);
                // 用白色清空 
                g.Clear(Color.White);
                // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // 指定高质量、低速度呈现。 
                g.SmoothingMode = SmoothingMode.HighQuality;
                // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
                g.DrawImage(imageFrom, new Rectangle(X, Y, bitmapWidth, bitmapHeight), new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //经测试 .jpg 格式缩略图大小与质量等最优 
                        bmp.Save(ms, ImageFormat.Jpeg);
                        ms.Seek(0, SeekOrigin.Begin);

                        return ms.ToArray();
                    }
                }
                finally
                {
                    //显示释放资源 
                    bmp.Dispose();
                    g.Dispose();
                }
            }
            //原图与模版比例不等，裁剪后缩放
            else
            {
                //裁剪对象
                Image pickedImage = null;
                Graphics pickedG = null;

                //定位
                Rectangle fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
                Rectangle toR = new Rectangle(0, 0, 0, 0);//目标定位

                //宽为标准进行裁剪
                if (templateRate > initRate)
                {
                    //裁剪对象实例化
                    pickedImage = new Bitmap(imageFrom.Width, (int)Math.Floor(imageFrom.Width / templateRate));
                    pickedG = Graphics.FromImage(pickedImage);

                    //裁剪源定位
                    fromR.X = 0;
                    fromR.Y = (int)Math.Floor((imageFrom.Height - imageFrom.Width / templateRate) / 2);
                    fromR.Width = imageFrom.Width;
                    fromR.Height = (int)Math.Floor(imageFrom.Width / templateRate);

                    //裁剪目标定位
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = imageFrom.Width;
                    toR.Height = (int)Math.Floor(imageFrom.Width / templateRate);
                }
                //高为标准进行裁剪
                else
                {
                    pickedImage = new Bitmap((int)Math.Floor(imageFrom.Height * templateRate), imageFrom.Height);
                    pickedG = Graphics.FromImage(pickedImage);

                    fromR.X = (int)Math.Floor((imageFrom.Width - imageFrom.Height * templateRate) / 2);
                    fromR.Y = 0;
                    fromR.Width = (int)Math.Floor(imageFrom.Height * templateRate);
                    fromR.Height = imageFrom.Height;

                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = (int)Math.Floor(imageFrom.Height * templateRate);
                    toR.Height = imageFrom.Height;
                }

                //设置质量
                pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = SmoothingMode.HighQuality;

                //裁剪
                pickedG.DrawImage(imageFrom, toR, fromR, GraphicsUnit.Pixel);

                //按模版大小生成最终图片
                Image templateImage = new Bitmap(width, height);
                Graphics templateG = Graphics.FromImage(templateImage);
                templateG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                templateG.SmoothingMode = SmoothingMode.HighQuality;
                templateG.Clear(Color.White);
                templateG.DrawImage(pickedImage, new Rectangle(0, 0, width, height), new Rectangle(0, 0, pickedImage.Width, pickedImage.Height), GraphicsUnit.Pixel);
                //ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
                //EncoderParameters ep = new EncoderParameters(1);
                //ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);

                ////保存缩略图
                //templateImage.Save(destFile, ici, ep);//ImageFormat.Jpeg);
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //经测试 .jpg 格式缩略图大小与质量等最优 
                        templateImage.Save(ms, ImageFormat.Jpeg);
                        ms.Seek(0, SeekOrigin.Begin);

                        return ms.ToArray();
                    }
                }
                finally
                {
                    //释放资源
                    templateG.Dispose();
                    templateImage.Dispose();

                    pickedG.Dispose();
                    pickedImage.Dispose();
                }
            }
        }
        //相对路径转换成服务器本地物理路径  
        public static string urlTolocal(string imagesurl1)
        {
            //string tmpRootDir = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.ToString());//获取程序根目录  
            string imagesurl2 = imagesurl1.Replace(@"/", @"\"); //转换成绝对路径  
            return imagesurl2;
        }

        public static bool CheckClientIsMobile()
        {
            //判断是否是移动端访问
            System.Web.HttpBrowserCapabilities myBrowserCaps = HttpContext.Current.Request.Browser;
            bool isMobileDevice = ((System.Web.Configuration.HttpCapabilitiesBase)myBrowserCaps).IsMobileDevice;
            return isMobileDevice;
        }
        public static string AjaxDeCode(string str)
        {
            str = str.Replace("{￥bai￥}", "%");
            str = str.Replace("{￥dan￥}", "'");
            str = str.Replace("{￥shuang￥}", "\"");
            str = str.Replace("{￥kong￥}", " ");
            str = str.Replace("{￥zuojian￥}", "<");
            str = str.Replace("{￥youjian￥}", ">");
            str = str.Replace("{￥and￥}", "&");
            str = str.Replace("{￥tab￥}", "\t");
            str = str.Replace("{￥jia￥}", "+");
            return str;
        }

        /// <summary>
        /// 发生异常时发送异常详情到个人邮箱
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="error">错误码</param>
        /// <param name="msg">错误详情</param>
        public static void SendErrorLogEmail(HttpContextBase context, string error, string msg)
        {
            var mg = new MailMessage();
            mg.To.Add("372364996@qq.com");
            mg.From = new MailAddress("372364996@qq.com", "错误码" + error, System.Text.Encoding.UTF8);
            mg.Subject = "异常发生URL:" + context.Request.Url;//邮件标题 
            mg.SubjectEncoding = Encoding.UTF8;//邮件标题编码 
            mg.Body = msg;//邮件内容 
            mg.BodyEncoding = Encoding.UTF8;//邮件内容编码 
            mg.IsBodyHtml = false;//是否是HTML邮件 
            mg.Priority = MailPriority.High;//邮件优先级
            var client = new SmtpClient
            {
                Credentials = new System.Net.NetworkCredential("372364996@qq.com", "dupeng1484"),
                Host = "smtp.qq.com"
            };
            try
            {
                if (context.Request.Url == null || context.Request.Url.Host != "www.weikebang.com" && context.Request.Url.Host != "weikebang.com")
                    return;
                logger.Debug($"网站Host:{context.Request.Url.Host}");
                client.Send(mg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encode">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        public static string EncodeBase64(Encoding encode, string source)
        {
            string enString = "";
            byte[] bytes = encode.GetBytes(source);
            try
            {
                enString = Convert.ToBase64String(bytes);
            }
            catch
            {
                enString = source;
            }
            return enString;
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encode">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string DecodeBase64(Encoding encode, string result)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
        /// <summary>
        /// 字节转化成图片
        /// </summary>
        /// <param name="byt"></param>
        /// <returns></returns>
        public static Image BytToImg(byte[] byt)
        {
            MemoryStream ms = new MemoryStream(byt);
            Image img = Image.FromStream(ms);
            return img;
        }
    }
    /// <summary>
    /// WebClient派生类，重写GetWebRequest解决TimeOut问题
    /// </summary>
    public class NewWebClient : WebClient
    {
        private int _timeout;

        /// <summary>
        /// 超时时间(毫秒)
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public NewWebClient()
        {
            this._timeout = 60000;
        }

        public NewWebClient(int timeout)
        {
            this._timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this._timeout;
            return result;
        }
    }
}