using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    internal class EnvironmentAccessor : Accessor, IEnvironmentAccessor
    {
        public EnvironmentAccessor(IScopeProvider scopeProvider) : base(scopeProvider)
        {
        }

        public async Task<bool> Create(Dtos.Environment environment)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Dtos.Environment>> Read()
        {
            throw new NotImplementedException();
        }

        public async Task<Dtos.Environment> Read(Guid key)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(Dtos.Environment environment)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Delete(Guid key)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Delete(Dtos.Environment environment)
        {
            throw new NotImplementedException();
        }
    }
}
