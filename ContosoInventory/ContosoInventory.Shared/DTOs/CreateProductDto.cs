using System.ComponentModel.DataAnnotations;

namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Request data for creating a product.
/// </summary>
public class CreateProductDto
{
    /// <summary>Product display name.</summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit identifier (unique, case-insensitive).</summary>
    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Unit price.</summary>
    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal Price { get; set; }

    /// <summary>Current stock level.</summary>
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>Identifier of the associated category.</summary>
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
