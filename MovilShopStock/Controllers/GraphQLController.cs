using GraphQL;
using GraphQL.Types;
using MovilShopStock.Models.GraphQL.Querys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MovilShopStock.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Route("graphql")]
    public class GraphQLController : ApiController
    {
        [System.Web.Mvc.HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] GraphQLQuery query)
        {
            var schema = new Schema { Query = new CategoryQuery(), Mutation = new CategoryMutation() };
            var inputs = query.Variables.ToInputs();
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Inputs = inputs;
                _.Schema = schema;
                _.Query = query.Query;
            }).ConfigureAwait(false);

            if (result.Errors?.Count > 0)
            {
                return BadRequest();
            }

            return Ok(result);
        }
    }
}