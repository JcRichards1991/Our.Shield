using System;

namespace Shield.UmbracoAccess.Persistance
{
    /// <summary>
    /// The Umbraco Access Configuration Context.
    /// </summary>
    public class ConfigurationContext : Core.Persistance.Bal.ConfigurationContext
    {
        internal static readonly Guid id = new Guid("fa3bfb2e-b98d-40c3-ab2c-fa33b27f89ba");

        /// <summary>
        /// The Id of the Configuration Context.
        /// </summary>
        public override Guid Id { get { return id; } }

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