using Microsoft.AspNet.Identity;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovilShopStock.Models.Handlers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //base.OnAuthorization(filterContext);
            bool flag = false;
            string UserId;

            //Check if Http Context
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(Roles))
                {
                    return;
                }

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

                    if (Roles.Contains(businessUser.Role.Name))
                    {
                        flag = true;
                    }
                }

                string[] roles = this.Roles.Split(',');
            }

            if (flag == false)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}