using System;
using System.Collections.Generic;

namespace Netpower.CustomerOrders.Domain.Entities;

public partial class Orders
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public byte Status { get; set; }

    public DateTime OrderDateUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Customers Customer { get; set; } = null!;
}
