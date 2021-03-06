﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MovilShopStock.Models.Catalog
{
    public class User : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        [ForeignKey("CurrentBusiness")]
        public Guid? CurrentBusiness_Id { get; set; }

        public virtual Business CurrentBusiness { get; set; }
        public virtual List<StockIn> StockIns { get; set; }
        public virtual List<BusinessUser> BusinessUsers { get; set; }
        public virtual List<StockOut> StockOuts { get; set; }
        public virtual List<TransferBusinessProduct> TransferBusinessProducts { get; set; }
        public virtual List<TransferMoneyUser> TransferMoneyUsers { get; set; }
    }
}