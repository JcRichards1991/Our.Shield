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
        /// Saves a new <see cref="Dtos.IEnvironment"/> to the database
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        Task<bool> Create(Dtos.Environment environment);

        /// <summary>
        /// Reads a <see cref="Dtos.Environment"/> from the database
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<Dtos.Environment>> Read();

        /// <summary>
        /// Gets the <see cref="Dtos.Environment"/> from the database by it's Key
        /// </summary>
        /// <param name="key">The Key of the <see cref="Dtos.Environment"/> to retrieve</param> 
        /// <returns></returns>
        Task<Dtos.Environment> Read(Guid key);

        /// <summary>
        /// Updates an <see cref="Dtos.IEnvironment"/> in the database
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        Task<bool> Update(Dtos.Environment environment);

        /// <summary>
        /// Deletes an <see cref="Dtos.IEnvironment"/> from the database by it's Key
        /// </summary>
        /// <param name="key">The key of the <see cref="Dtos.Environment"/> to delete</param>
        /// <returns></returns>
        Task<bool> Delete(Guid key);

        /// <summary>
        /// Deletes an <see cref="Dtos.Environment"/> from the database
        /// </summary>
        /// <param name="environment">The <see cref="Dtos.IEnvironment"/> to delete</param>
        /// <returns></returns>
        Task<bool> Delete(Dtos.Environment environment);
    }
}
