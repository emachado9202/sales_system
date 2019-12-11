using GraphQL.Types;
using MovilShopStock.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.GraphQL.Querys
{
    public class CategoryQuery : ObjectGraphType
    {
        public CategoryQuery()

        {
            Field<CategoryType>("category",
                  arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }),
                  resolve: context => CategoryService.Get(context.GetArgument<int>("id")));
            Field<CategoryType>("categories",
                 arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }),
                 resolve: context => CategoryService.));
        }
    }
}