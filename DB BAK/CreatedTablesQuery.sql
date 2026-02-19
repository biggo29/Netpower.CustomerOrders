-- Customers
CREATE TABLE dbo.Customers (
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    CustomerNumber   NVARCHAR(30)      NOT NULL,  -- business-friendly id
    FirstName        NVARCHAR(100)     NOT NULL,
    LastName         NVARCHAR(100)     NOT NULL,
    Email            NVARCHAR(320)     NOT NULL,
    Phone            NVARCHAR(30)      NULL,

    -- Optional: keep address as separate fields (better for validation/search)
    AddressLine1     NVARCHAR(200)     NULL,
    AddressLine2     NVARCHAR(200)     NULL,
    City             NVARCHAR(100)     NULL,
    State            NVARCHAR(100)     NULL,
    PostalCode       NVARCHAR(30)      NULL,
    Country          NVARCHAR(100)     NULL,

    -- GDPR / lifecycle
    IsDeleted        BIT               NOT NULL CONSTRAINT DF_Customers_IsDeleted DEFAULT (0),
    DeletedAtUtc     DATETIME2(3)      NULL,

    -- Auditing
    CreatedAtUtc     DATETIME2(3)      NOT NULL CONSTRAINT DF_Customers_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    CreatedBy        NVARCHAR(100)     NULL,
    UpdatedAtUtc     DATETIME2(3)      NULL,
    UpdatedBy        NVARCHAR(100)     NULL,

    -- Concurrency
    RowVersion       ROWVERSION        NOT NULL
);

-- Uniqueness constraints
ALTER TABLE dbo.Customers
ADD CONSTRAINT UQ_Customers_CustomerNumber UNIQUE (CustomerNumber);

-- Email uniqueness: you can decide case-insensitive logic by collation or normalized column
ALTER TABLE dbo.Customers
ADD CONSTRAINT UQ_Customers_Email UNIQUE (Email);


-- Orders
CREATE TABLE dbo.Orders (
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    OrderNumber      NVARCHAR(30)      NOT NULL,

    CustomerId       UNIQUEIDENTIFIER  NOT NULL,
    Status           TINYINT           NOT NULL,  -- maps to enum in .NET (e.g., 0=Draft,1=Placed,2=Paid,3=Shipped,4=Cancelled)
    OrderDateUtc     DATETIME2(3)      NOT NULL,  -- business date/time
    TotalAmount      DECIMAL(18,2)     NOT NULL CONSTRAINT CK_Orders_TotalAmount_NonNegative CHECK (TotalAmount >= 0),

    Notes            NVARCHAR(1000)    NULL,      -- be careful: PII should NOT be placed here

    -- GDPR / lifecycle
    IsDeleted        BIT               NOT NULL CONSTRAINT DF_Orders_IsDeleted DEFAULT (0),
    DeletedAtUtc     DATETIME2(3)      NULL,

    -- Auditing
    CreatedAtUtc     DATETIME2(3)      NOT NULL CONSTRAINT DF_Orders_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    CreatedBy        NVARCHAR(100)     NULL,
    UpdatedAtUtc     DATETIME2(3)      NULL,
    UpdatedBy        NVARCHAR(100)     NULL,

    RowVersion       ROWVERSION        NOT NULL,

    CONSTRAINT FK_Orders_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
);

ALTER TABLE dbo.Orders
ADD CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber);
