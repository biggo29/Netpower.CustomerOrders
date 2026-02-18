-- Fast lookup of active customers by email (also helps login/identity-like checks if needed)
CREATE INDEX IX_Customers_Email_IsDeleted
ON dbo.Customers (Email)
INCLUDE (FirstName, LastName, CustomerNumber)
WHERE IsDeleted = 0;

-- Orders: typical filters: CustomerId, Status, date range, plus paging/sorting by date
CREATE INDEX IX_Orders_CustomerId_OrderDateUtc
ON dbo.Orders (CustomerId, OrderDateUtc DESC)
INCLUDE (Status, TotalAmount, OrderNumber)
WHERE IsDeleted = 0;

-- If endpoint supports global filtering by status + date range (not only per customer)
CREATE INDEX IX_Orders_Status_OrderDateUtc
ON dbo.Orders (Status, OrderDateUtc DESC)
INCLUDE (CustomerId, TotalAmount, OrderNumber)
WHERE IsDeleted = 0;
