﻿using EFCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.Handlers
{
    public class CacheManager
    {
        public static readonly InMemoryCache Stock = new InMemoryCache();
    }
}