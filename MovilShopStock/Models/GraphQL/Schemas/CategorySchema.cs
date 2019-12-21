using GraphQL;
using GraphQL.Types;
using MovilShopStock.Models.GraphQL.Querys;

namespace MovilShopStock.Models.GraphQL.Schemas
{
    public class CategorySchema : Schema
    {
        public CategorySchema(IDependencyResolver resolve)
        {
            Query = resolve.Resolve<CategoryQuery>();
        }
    }
}