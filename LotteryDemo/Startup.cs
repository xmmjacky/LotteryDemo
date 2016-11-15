using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LotteryDemo.Startup))]
namespace LotteryDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
