using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    internal interface IAppAccessor
    {
        Task<bool> Create(App app);

        Task<IReadOnlyList<App>> Read();

        Task<App> Read(Guid key);

        Task<bool> Update(App App);
    }
}
