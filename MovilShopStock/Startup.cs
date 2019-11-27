using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MovilShopStock.Startup))]
namespace MovilShopStock
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
