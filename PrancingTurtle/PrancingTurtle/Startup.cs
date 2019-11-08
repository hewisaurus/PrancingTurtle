using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PrancingTurtle.Startup))]
namespace PrancingTurtle
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            ConfigureAuth(app);
        }
    }
}
