using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MovilShopStock.Controllers
{
    [Authorize]
    public class BusinessController : GenericController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}