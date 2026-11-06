using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Base
{
    public interface IReadRepository<T>
    {
        public Task<T> ReadAsync(Guid id);
    }
}
