using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
