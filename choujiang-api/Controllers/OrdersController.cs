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
    public class OrdersController : Controller
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
            var data = db.Orders.SingleOrDefault(o => o.PorductId == id && user.OpenId == openid);
            if (data != null)
            {
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
    }
}
