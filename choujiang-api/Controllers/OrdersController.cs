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
    }
}
