using GraphQL;
using GraphQL.Types;

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