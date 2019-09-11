using Swashbuckle.Application;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(Our.Shield.Swagger.Operation.SwaggerConfig), "Register")]

namespace Our.Shield.Swagger.Operation
{
    public class SwaggerConfig
    {

        private static string GetXmlCommentsPath()
        {
            /*
                TODO: Come up with way of handling this correctly, otherwise, Executing Assembly is Our.Shield.Swagger
                so would be getting the wrong comments, not website's comments.
                Also, the website may not have ApiControllers, will need a way of defining this and pulling the desired xml file
            */
            return $"{AppDomain.CurrentDomain.BaseDirectory}bin\\{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        }

        /// <summary>
        /// Entry point that is called on application startup
        /// </summary>
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Website");
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                    c.DocumentFilter<FilterBuiltinUrl>();
                    //c.IncludeXmlComments(GetXmlCommentsPath());
                })
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator();
                });
        }
    }
}
