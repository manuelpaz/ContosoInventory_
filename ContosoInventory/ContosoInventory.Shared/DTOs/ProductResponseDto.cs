namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Product data returned by the API.
/// </summary>
public class ProductResponseDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Product display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit identifier.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    public string? Description { get; set; }

    /// <summary>Unit price.</summary>
    public decimal Price { get; set; }

    /// <summary>Current stock level.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Identifier of the associated category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Name of the associated category.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Last modification timestamp (UTC).</summary>
    public DateTime LastUpdatedDate { get; set; }
}
