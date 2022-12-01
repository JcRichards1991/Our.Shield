using Swashbuckle.Application;
using System;
using System.Web.Http;

namespace Our.Shield.Swagger.Extensions
{
    public static class HttpConfigurationExtensions
    {
        private static Action<SwaggerDocsConfig> _swaggerDocsConfig { get; set; }

        public static void SetSwaggerDocsConfig(
            this HttpConfiguration _,
            Action<SwaggerDocsConfig> swaggerDocsConfig)
        {
            _swaggerDocsConfig = swaggerDocsConfig
                ?? throw new ArgumentNullException($"{nameof(swaggerDocsConfig)} cannot be null.");
        }

        internal static Action<SwaggerDocsConfig> GetSwaggerDocsConfig(this HttpConfiguration _)
        {
            if (_swaggerDocsConfig == null)
            {
                throw new NullReferenceException($"Swagger Docs Config is null. Ensure {nameof(SetSwaggerDocsConfig)} is called first.");
            }

            return _swaggerDocsConfig;
        }

        private static Action<SwaggerUiConfig> _swaggerUiConfig { get; set; }

        public static void SetSwaggerUiConfig(
            this HttpConfiguration _,
            Action<SwaggerUiConfig> swaggerUiConfig)
        {
            _swaggerUiConfig = swaggerUiConfig
                ?? throw new ArgumentNullException($"{nameof(swaggerUiConfig)} cannot be null.");
        }

        internal static Action<SwaggerUiConfig> GetSwaggerUiConfig(this HttpConfiguration _)
        {
            if (_swaggerUiConfig == null)
            {
                throw new NullReferenceException($"Swagger UI Config is null. Ensure {nameof(SetSwaggerUiConfig)} is called first.");
            }

            return _swaggerUiConfig;
        }
    }
}
