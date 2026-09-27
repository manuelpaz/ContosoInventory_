using System.ComponentModel.DataAnnotations;

namespace ContosoInventory.Shared.DTOs;

public class UpdateProductDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Sku { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int? ReorderLevel { get; set; }

    public bool? IsActive { get; set; }

    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }
}
