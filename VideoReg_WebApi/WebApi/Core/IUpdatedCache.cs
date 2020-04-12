using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace WebApi.Core
{
    public interface IUpdatedCache
    {
        public void BeginUpdate();
    }
}
