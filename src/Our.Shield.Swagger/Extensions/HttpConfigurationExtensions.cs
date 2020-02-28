using Swashbuckle.Application;
using System;
using System.Web.Http;

namespace Our.Shield.Swagger.Extensions
{
    public static class HttpConfigurationExtensions
    {
        private static Action<SwaggerDocsConfig> _swaggerDocsConfig { get; set; }

        public static void SetSwaggerDocsConfig(this HttpConfiguration configuration, Action<SwaggerDocsConfig> swaggerDocsConfig)
        {
            if (swaggerDocsConfig == null)
                throw new ArgumentNullException($"{nameof(swaggerDocsConfig)} cannot be null.");

            _swaggerDocsConfig = swaggerDocsConfig;
        }

        internal static Action<SwaggerDocsConfig> GetSwaggerDocsConfig(this HttpConfiguration configuration)
        {
            if (_swaggerDocsConfig == null)
                throw new NullReferenceException($"Swagger Docs Config is null. Ensure {nameof(SetSwaggerDocsConfig)} is called first.");

            return _swaggerDocsConfig;
        }

        private static Action<SwaggerUiConfig> _swaggerUiConfig { get; set; }

        public static void SetSwaggerUiConfig(this HttpConfiguration configuration, Action<SwaggerUiConfig> swaggerUiConfig)
        {
            if (swaggerUiConfig == null)
                throw new ArgumentNullException($"{nameof(swaggerUiConfig)} cannot be null.");

            _swaggerUiConfig = swaggerUiConfig;
        }

        internal static Action<SwaggerUiConfig> GetSwaggerUiConfig(this HttpConfiguration configuration)
        {
            if (_swaggerUiConfig == null)
                throw new NullReferenceException($"Swagger UI Config is null. Ensure {nameof(SetSwaggerUiConfig)} is called first.");

            return _swaggerUiConfig;
        }
    }
}
