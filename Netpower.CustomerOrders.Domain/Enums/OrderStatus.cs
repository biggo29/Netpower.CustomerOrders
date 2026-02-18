using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Domain.Enums
{
    public enum OrderStatus : byte
    {
        Draft = 0,
        Placed = 1,
        Paid = 2,
        Shipped = 3,
        Cancelled = 4
    }
}
