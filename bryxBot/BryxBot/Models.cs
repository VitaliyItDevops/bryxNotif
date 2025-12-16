namespace BryxBot;

public class ProductsResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsDefective { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalesResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<SaleDto> Sales { get; set; } = new();
}

public class SaleDto
{
    public int Id { get; set; }
    public string Buyer { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TTN { get; set; }
    public string? SoldThrough { get; set; }
    public string? AdditionalService { get; set; }
    public int ProductCount { get; set; }
}

public class StatsResponse
{
    public ProductStats Products { get; set; } = new();
    public SalesStats Sales { get; set; } = new();
    public List<CategoryStats> Categories { get; set; } = new();
}

public class ProductStats
{
    public int Total { get; set; }
    public int InStock { get; set; }
    public int Sold { get; set; }
    public int Expected { get; set; }
}

public class SalesStats
{
    public int Total { get; set; }
    public decimal TotalAmount { get; set; }
    public TodayStats Today { get; set; } = new();
}

public class TodayStats
{
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class CategoryStats
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}
