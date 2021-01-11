using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    internal interface IEnvironmentAccessor
    {
        Task<bool> Create(Dtos.Environment environment);

        Task<IList<Dtos.Environment>> Read();

        Task<Dtos.Environment> Read(Guid key);

        Task<bool> Update(Dtos.Environment environment);

        Task<bool> Delete(Guid key);

        Task<bool> Delete(Dtos.Environment environment);
    }
}
