using System.ComponentModel.DataAnnotations;

namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Request data for adding stock to a product.
/// </summary>
public class RestockProductDto
{
    /// <summary>Number of units to add (1 to 100000).</summary>
    [Range(1, 100000)]
    public int Quantity { get; set; }
}
