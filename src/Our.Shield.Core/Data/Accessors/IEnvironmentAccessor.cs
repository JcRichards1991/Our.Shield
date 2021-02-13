using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEnvironmentAccessor
    {
        /// <summary>
        /// Saves a new <see cref="IEnvironment"/> to the database
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        Task<Guid> Create(IEnvironment environment);

        /// <summary>
        /// Reads a <see cref="IEnvironment"/> from the database
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<Dtos.Environment>> Read();

        /// <summary>
        /// Gets the <see cref="IEnvironment"/> from the database by it's Key
        /// </summary>
        /// <param name="key">The Key of the <see cref="IEnvironment"/> to retrieve</param> 
        /// <returns></returns>
        Task<Dtos.Environment> Read(Guid key);

        /// <summary>
        /// Updates an <see cref="IEnvironment"/> in the database
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        Task<bool> Update(IEnvironment environment);

        /// <summary>
        /// Deletes an <see cref="IEnvironment"/> from the database
        /// </summary>
        /// <param name="key">Key of the <see cref="IEnvironment"/> to delete</param>
        /// <returns></returns>
        Task<bool> Delete(Guid key);
    }
}
