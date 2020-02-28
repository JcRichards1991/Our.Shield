using System.Web.Http;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(Our.Shield.Swagger.Test.Startup), nameof(Our.Shield.Swagger.Test.Startup.Register))]
namespace Our.Shield.Swagger.Test
{
    public class Startup
    {
        public static void Register()
        {
            GlobalConfiguration
                .Configuration
                .Routes
                .MapHttpRoute(
                    "TestApi",
                    "Api/TestApi/{id}",
                    new { controller = "TestApi", id = RouteParameter.Optional });
        }
    }
}
