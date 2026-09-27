using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoInventory.Server.Models;

/// <summary>
/// Represents a product in the inventory, associated with a single category.
/// </summary>
public class Product
{
    /// <summary>Unique identifier.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Product display name.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit identifier, stored normalized (trimmed, upper case).</summary>
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Unit price.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>Current stock level.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Foreign key to the associated category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Associated category.</summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Last modification timestamp (UTC).</summary>
    public DateTime LastUpdatedDate { get; set; }
}
