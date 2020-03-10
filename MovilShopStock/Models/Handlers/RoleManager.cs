using Microsoft.AspNet.Identity;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.Handlers
{
    public class RoleManager
    {
        public const string Administrator = "Administrador";
        public const string Editor = "Editor";
        public const string Dealer = "Vendedor";

        public static bool IsInRole(string role)
        {
            bool flag = false;
            string UserId;

            //Check if Http Context
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                ApplicationDbContext applicationDbContext = new ApplicationDbContext();

                //Get User Id from HttpContext
                UserId = HttpContext.Current.User.Identity.GetUserId();
                User user = applicationDbContext.Users.Include("BusinessUsers").Include("BusinessUsers.Role").Include("BusinessUsers.Business").FirstOrDefault(x => x.Id == UserId);

                if (user != null)
                {
                    BusinessUser businessUser = user.BusinessUsers.FirstOrDefault(x => x.Business_Id == user.CurrentBusiness_Id);

                    if (businessUser == null)
                    {
                        businessUser = user.BusinessUsers.FirstOrDefault(x => x.Business.IsPrimary);
                    }

                    if (role.Equals(businessUser.Role.Name))
                    {
                        flag = true;
                    }
                }
            }

            return flag;
        }
    }
}