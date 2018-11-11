using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Our.Shield.Core.Persistence.Business;

namespace Our.Shield.Core.Services
{
    internal class EnvironmentService
    {
        private static readonly Lazy<EnvironmentService> EnvironmentServiceInstance = new Lazy<EnvironmentService>(() => new EnvironmentService());

        private EnvironmentService()
        {
        }

        /// <summary>
        /// Accessor for instance
        /// </summary>
        public static EnvironmentService Instance => EnvironmentServiceInstance.Value;

        /// <summary>
        /// Writes an environment to the database
        /// </summary>
        /// <param name="environment">The environment to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        public bool Write(IEnvironment environment)
        {
            if (!DbContext.Instance.Environment.Write(environment))
            {
                return false;
            }
            
            if (!JobService.Instance.Environments.Any(x => x.Key.Id.Equals(environment.Id)))
            {
                //created new environment, we need to register it
                JobService.Instance.Register(environment, Umbraco.Core.ApplicationContext.Current);

                var environments = DbContext.Instance.Environment.Read().Select(x => new Models.Environment(x));
                var oldEnvironments = JobService.Instance.Environments.Keys;

                foreach (var newEnv in environments)
                {
                    if (!oldEnvironments.Any(x => x.Id.Equals(newEnv.Id) && !x.SortOrder.Equals(newEnv.SortOrder)))
                        continue;

                    JobService.Instance.Unregister(newEnv);
                    JobService.Instance.Register(newEnv);
                }
            }
            else
            {
                //Environment has changed, we need to unregister it
                //and then re-register it with the new changes
                if (!JobService.Instance.Unregister(environment))
                {
                    return false;
                }

                JobService.Instance.Register(environment);
            }

            JobService.Instance.Poll(true);
            return true;
        }

        /// <summary>
        /// Deletes an environment from the database
        /// </summary>
        /// <param name="environment">The environment to remove</param>
        /// <returns></returns>
        public bool Delete(Models.Environment environment)
        {
            if (!JobService.Instance.Unregister(environment) || !DbContext.Instance.Environment.Delete(environment.Id))
            {
                return false;
            }

            var environments = DbContext.Instance.Environment.Read().Select(x => new Models.Environment(x));
            var oldEnvironments = JobService.Instance.Environments.Keys;

            foreach (var newEnv in environments)
            {
                if (oldEnvironments.Any(x => x.Id.Equals(newEnv.Id) && !x.SortOrder.Equals(newEnv.SortOrder)))
                {
                    JobService.Instance.Unregister(newEnv);
                    JobService.Instance.Register(newEnv);
                }
            }

            JobService.Instance.Poll(true);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id of the environment to return the journals for</param>
        /// <param name="page">The page of results to return</param>
        /// <param name="itemsPerPage">The number of items to return per page</param>
        /// <param name="type">The type of journal the the results should be</param>
        /// <param name="totalPages">The total amount of pages that can be returned</param>
        /// <returns></returns>
        public IEnumerable<IJournal> JournalListing(int id, int page, int itemsPerPage, Type type, out int totalPages) =>
            DbContext.Instance.Journal.Read(id, page, itemsPerPage, type, out totalPages);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The type of journal the the results should be</typeparam>
        /// <param name="id">Id of the environment to return the journals for</param>
        /// <param name="page">The page of results to return</param>
        /// <param name="itemsPerPage">The number of items to return per page</param>
        /// <param name="totalPages">The total amount of pages that can be returned</param>
        /// <returns></returns>
        public IEnumerable<T> JournalListing<T>(int id, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            DbContext.Instance.Journal.Read(id, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);
    }
}
