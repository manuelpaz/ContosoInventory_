using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Defines operations for managing inventory products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products ordered by name, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">When provided, only products in this category are returned.</param>
    /// <returns>A list of product DTOs.</returns>
    Task<List<ProductResponseDto>> GetAllAsync(int? categoryId = null);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product DTO, or null if not found.</returns>
    Task<ProductResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the referenced category does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a product with the same SKU already exists.</exception>
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);

    /// <summary>
    /// Replaces all editable fields of an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the referenced category does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when another product already uses the SKU.</exception>
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto);

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>True if the product was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Increases the stock level of a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="quantity">The number of units to add; must be greater than zero.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is not positive or the resulting stock would overflow.</exception>
    Task<ProductResponseDto?> RestockAsync(int id, int quantity);
}
