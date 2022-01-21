using Our.Shield.Core.Models.AppTabs;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// Response for an <see cref="App{TC}"/> with it's <see cref="IAppConfiguration"/>
    /// </summary>
    public class GetAppResponse : BaseResponse
    {
        /// <summary>
        /// The App
        /// </summary>
        public IApp App { get; set; }

        /// <summary>
        /// The Configuration for the <see cref="App"/>
        /// </summary>
        public IAppConfiguration Configuration { get; set; }

        /// <summary>
        /// The Tabs for rendering in the UI
        /// </summary>
        public IEnumerable<ITab> Tabs { get; set; }
    }
}
