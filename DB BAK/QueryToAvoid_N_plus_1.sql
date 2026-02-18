-- Example: list orders by customer with optional status/date filters and pagination
SELECT
    o.Id,
    o.OrderNumber,
    o.CustomerId,
    o.Status,
    o.OrderDateUtc,
    o.TotalAmount
FROM dbo.Orders o
WHERE
    o.IsDeleted = 0
    AND o.CustomerId = @CustomerId
    AND (@Status IS NULL OR o.Status = @Status)
    AND (@FromUtc IS NULL OR o.OrderDateUtc >= @FromUtc)
    AND (@ToUtc IS NULL OR o.OrderDateUtc <  @ToUtc)
ORDER BY o.OrderDateUtc DESC, o.Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
