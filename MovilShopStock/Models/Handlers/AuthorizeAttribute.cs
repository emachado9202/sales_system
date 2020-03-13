﻿using Microsoft.AspNet.Identity;
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
            var custom_attributes = filterContext.ActionDescriptor.GetCustomAttributes(true);

            if (custom_attributes.FirstOrDefault(x => x.GetType().Name == "AllowAnonymousAttribute") != null)
            {
                return;
            }

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

                string[] roles = this.Roles.Split(',');

                foreach (var role in roles)
                {
                    if (RoleManager.IsInRole(role))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag == false)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}