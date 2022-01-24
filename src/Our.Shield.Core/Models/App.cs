using Newtonsoft.Json;
using System;
using System.Diagnostics;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <typeparam name="TC">The type of configuration for the app</typeparam>
    [DebuggerDisplay("Id: {Id}; Key: {Key}")]
    public abstract class App<TC> : IApp where TC : IAppConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly IUmbracoContextAccessor UmbContextAccessor;

        /// <summary>
        /// 
        /// </summary>
        protected readonly ILocalizedTextService LocalizedTextService;

        /// <summary>
        /// 
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of <see cref="App{TC}"/>
        /// </summary>
        /// <param name="umbContextAccessor"><see cref="IUmbracoContextAccessor"/></param>
        /// <param name="localizedTextService"><see cref="ILocalizedTextService"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        public App(
            IUmbracoContextAccessor umbContextAccessor,
            ILocalizedTextService localizedTextService,
            ILogger logger)
        {
            UmbContextAccessor = umbContextAccessor;
            LocalizedTextService = localizedTextService;
            Logger = logger;
        }

        /// <inheritdoc />
        public Guid Key { get; set; }

        /// <inheritdoc />
        public abstract string Id { get; }

        /// <inheritdoc />
        public abstract string Icon { get; }

        /// <summary>
        /// The initialize method for the App
        /// </summary>
        /// <returns>True if successfully initialized; Otherwise, False</returns>
        public virtual bool Init() => true;

        /// <inheritdoc />
        [JsonIgnore]
        public virtual IAppConfiguration DefaultConfiguration => default(TC);

        /// <inheritdoc />
        public virtual bool Execute(IJob job, IAppConfiguration config) => true;

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is App<IAppConfiguration> otherApp))
            {
                return false;
            }

            return Id == otherApp.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Id.GetHashCode();
    }
}
