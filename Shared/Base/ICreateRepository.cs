using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Base
{
    public interface ICreateRepository<T>
    {
        public Task CreateAsync(T entity);
    }
}
