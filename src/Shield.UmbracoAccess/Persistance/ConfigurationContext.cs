using System;

namespace Shield.UmbracoAccess.Persistance
{
    /// <summary>
    /// The Umbraco Access Configuration Context.
    /// </summary>
    public class ConfigurationContext : Core.Persistance.Bal.ConfigurationContext
    {
        internal static readonly string _Id = nameof(Shield.UmbracoAccess);

        /// <summary>
        /// The Id of the Configuration Context.
        /// </summary>
        public override string Id { get { return _Id; } }

        /// <summary>
        /// Reads the Configuration from the database.
        /// </summary>
        /// <returns>
        /// Configuration object.
        /// </returns>
        public Configuration Read()
        {
            return Read<Configuration>();
        }

        /// <summary>
        /// Writes the Configuration to the database
        /// </summary>
        /// <param name="model">
        /// The Configuration object to write.
        /// </param>
        /// <returns>
        /// True if successfully written to the database, otherwise false.
        /// </returns>
        public bool Write(Configuration model)
        {
            return Write<Configuration>(model);
        }
    }
}