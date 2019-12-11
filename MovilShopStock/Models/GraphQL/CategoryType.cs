using GraphQL.Types;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.GraphQL
{
    public class CategoryType : ObjectGraphType<Category>
    {
        public CategoryType()
        {
            Field(x => x.Id);
            Field(x => x.Name);
            Field(x => x.LastUpdated);
            Field(x => x.ShowDashboard);
            Field(x => x.SystemAction);
        }
    }
}