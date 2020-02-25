using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Our.Shield.Site.Controllers.ApiControllers
{
    /// <summary>
    /// Test Api Controller Inheriting UmbracoApiController for Swagger UI to use
    /// </summary>
    public class TestUmbracoApiController : UmbracoApiController
    {
        private List<string> _listA
        {
            get
            {
                if (System.Web.HttpContext.Current.Application["listb"] == null)
                {
                    System.Web.HttpContext.Current.Application["listb"] = new List<string> { "value1", "value2" };
                }

                return (List<string>)System.Web.HttpContext.Current.Application["listb"];
            }
        }

        /// <summary>
        /// Retrieves the list of values
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _listA;
        }

        /// <summary>
        /// Retrieves one value from the list of values
        /// </summary>
        /// <param name="id">The id of the item to be retrieved</param>
        /// <returns></returns>
        [HttpGet]
        public string Get(int id)
        {
            return _listA[id];
        }

        /// <summary>
        /// Insert a new value in the list
        /// </summary>
        /// <param name="value">New value to be inserted</param>
        [HttpPost]
        public void Post(string value)
        {
            _listA.Add(value);
        }

        /// <summary>
        /// Change a single value in the list or adds a new value
        /// </summary>
        /// <param name="id">The id of the value to be changed</param>
        /// <param name="value">The new value</param>
        [HttpPut]
        public void Put(int id, string value)
        {
            if (id > 0 && id < _listA.Count - 1)
                _listA[id] = value;

            _listA.Add(value);
        }

        /// <summary>
        /// Delete an item from the list
        /// </summary>
        /// <param name="id">id of the item to be deleted</param>
        [HttpDelete]
        public void Delete(int id)
        {
            _listA.RemoveAt(id);
        }

        /// <summary>
        /// Patches a value to a new value in the list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPatch]
        public void Patch(int id, string value)
        {
            _listA[id] = value;
        }
    }
}
