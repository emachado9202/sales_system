using MovilShopStock.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovilShopStock.Controllers
{
    [Authorize]
    public class TransferController : GenericController
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }
    }
}