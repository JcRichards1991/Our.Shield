using Newtonsoft.Json;
using Our.Shield.Core.Services;
using System;
using System.Diagnostics;

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
        protected IJournalService JournalService;

        /// <summary>
        /// Initializes a new instance of <see cref="App{TC}"/>
        /// </summary>
        /// <param name="journalService"></param>
        public App(IJournalService journalService)
        {
            JournalService = journalService;
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
