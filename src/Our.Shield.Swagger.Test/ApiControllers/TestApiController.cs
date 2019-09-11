using System.Collections.Generic;
using System.Web.Http;

namespace Our.Shield.Swagger.Test.ApiControllers
{
    /// <summary>
    /// Test Api Controller for Swagger UI to use
    /// </summary>
    public class TestApiController : ApiController
    {
        private List<string> _listA
        {
            get
            {
                if (System.Web.HttpContext.Current.Application["lista"] == null)
                {
                    System.Web.HttpContext.Current.Application["lista" ] = new List<string> { "value1", "value2"  };
                }

                return (List<string>)System.Web.HttpContext.Current.Application["lista"];
            }
        }

        /// <summary>
        /// Retrieves the list of values
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Get()
        {
            return _listA;
        }

        /// <summary>
        /// Retrieves one value from the list of values
        /// </summary>
        /// <param name="id">The id of the item to be retrieved</param>
        /// <returns></returns>
        public string Get(int id)
        {
            return _listA[id];
        }

        /// <summary>
        /// Insert a new value in the list
        /// </summary>
        /// <param name="value">New value to be inserted</param>
        public void Post(string value)
        {
            _listA.Add(value);
        }

        /// <summary>
        /// Change a single value in the list
        /// </summary>
        /// <param name="id">The id of the value to be changed</param>
        /// <param name="value">The new value</param>
        public void Put(int id, string value)
        {
            _listA[id] = value;
        }

        /// <summary>
        /// Delete an item from the list
        /// </summary>
        /// <param name="id">id of the item to be deleted</param>
        public void Delete(int id)
        {
            _listA.RemoveAt(id);
        }
    }
}
