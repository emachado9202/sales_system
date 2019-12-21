using MovilShopStock.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Services
{
    public class GenericService
    {
        public static ApplicationDbContext applicationDbContext = new ApplicationDbContext();
    }
}