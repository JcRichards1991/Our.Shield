using Our.Shield.Core.Models;
using Our.Shield.Core.Persistance.Business;
using System;
using System.Linq;

namespace Our.Shield.Core.Operation
{
    internal class EnvironmentService
    {
        private static readonly Lazy<EnvironmentService> _instance = new Lazy<EnvironmentService>(() => new EnvironmentService());

        private EnvironmentService()
        {
        }

        /// <summary>
        /// Accessor for instance
        /// </summary>
        public static EnvironmentService Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Writes an environment to the database
        /// </summary>
        /// <param name="environment">the environment to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        public bool Write(IEnvironment environment)
        {
            var data = new Persistance.Data.Dto.Environment
            {
                Name = environment.Name,
                Icon = environment.Icon,
                Id = environment.Id,
                Domains = environment.Domains.Select(x => new Persistance.Data.Dto.Domain
                {
                    Id = x.Id,
                    Name = x.Name,
                    UmbracoDomainId = x.UmbracoDomainId
                }),
                SortOrder = environment.SortOrder,
                Enable = environment.Enable,
                ContinueProcessing = environment.ContinueProcessing
            };

            if(!DbContext.Instance.Environment.Write(data))
            {
                return false;
            }
            
            if (environment.Id == 0)
            {
                ((Models.Environment)environment).Id = data.Id;
                JobService.Instance.Register(environment, Umbraco.Core.ApplicationContext.Current);
            }

            JobService.Instance.Poll(true);
            return true;
        }

        /// <summary>
        /// Deletes an environment from the database
        /// </summary>
        /// <param name="environment">the environment to remove</param>
        /// <returns></returns>
        public bool Delete(Models.Environment environment)
        {
            if (!JobService.Instance.Unregister(environment) || !DbContext.Instance.Environment.Delete(environment.Id))
            {
                return false;
            }

            JobService.Instance.Poll(true);
            return true;
        }
        
    }
}
