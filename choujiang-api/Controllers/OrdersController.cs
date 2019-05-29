using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Components.Domains;
using Components.Repositories.Ef;

namespace choujiang_api.Controllers
{
    public class OrdersController : WxAppController
    {
        private ChouJiangDbContext db = new ChouJiangDbContext();

        // GET: Orders
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Products).Include(o => o.Users);
            return View(orders.ToList());
        }

        public ActionResult CreateOrder(int id, string openid)
        {

            var user = db.Users.SingleOrDefault(u => u.OpenId == openid);
            var data = db.Orders.SingleOrDefault(o => o.PorductId == id && o.Users.OpenId == openid);
            if (data != null)
            {
                logger.Debug("id:" + id);
                logger.Debug("openid:" + openid);
                logger.Debug("有订单:"+data.Products.Name);
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Order order = new Order();
                order.PorductId = id;
                order.UserId = user.Id;
                order.CreateTime = DateTime.Now;
                db.Orders.Add(order);
                db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetOrderListByUserId(string openid) {
            var list = db.Orders.ToList().Where(o => o.Users.OpenId == openid).Select(o=>new { o.Id,o.IsWin,o.Products.Name,OpenTime=o.Products.OpenTime.ToString("MM月dd日 HH:mm"), CreateTime = o.CreateTime.ToString("MM月dd日 HH:mm") }) ;
            return Json(new { list },JsonRequestBehavior.AllowGet);
        }
    }
}
