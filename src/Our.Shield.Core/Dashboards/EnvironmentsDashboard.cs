using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Our.Shield.Core.Dashboards
{
    /// <summary>
    /// Shield's Environment Dashboard
    /// </summary>
    [Weight(1)]
    public class EnvironmentsDashboard : IDashboard
    {
        /// <inheritdoc />
        public string[] Sections => new[] { Constants.App.Alias };

        /// <inheritdoc />
        public IAccessRule[] AccessRules => new IAccessRule[] { };

        /// <inheritdoc />
        public string Alias => "shieldEnvironmentsDashboard";

        /// <inheritdoc />
        public string View => "/App_Plugins/Shield/BackOffice/Dashboards/Environments.html?v=2.0.0";
    }
}
