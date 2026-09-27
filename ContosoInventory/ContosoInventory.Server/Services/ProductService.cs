using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;
using ContosoInventory.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory products.
/// </summary>
public class ProductService : IProductService
{
    private const string SkuConflictMessage = "The product could not be saved because another product already uses this SKU.";

    private readonly InventoryContext _context;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="context">The inventory database context.</param>
    /// <param name="logger">The logger.</param>
    public ProductService(InventoryContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductResponseDto>> GetAllAsync(int? categoryId = null)
    {
        try
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products (category filter: {CategoryId}).", categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product is null ? null : MapToResponse(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            await EnsureCategoryExistsAsync(dto.CategoryId);

            var sku = NormalizeSku(dto.Sku);
            if (await SkuExistsAsync(sku, excludeProductId: null))
            {
                throw new InvalidOperationException($"A product with the SKU '{sku}' already exists.");
            }

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = sku,
                Description = NormalizeDescription(dto.Description),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = now,
                LastUpdatedDate = now
            };

            _context.Products.Add(product);
            await SaveChangesWithConflictHandlingAsync(sku);

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return MapToResponse(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product '{ProductName}'.", dto.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return null;
            }

            await EnsureCategoryExistsAsync(dto.CategoryId);

            var sku = NormalizeSku(dto.Sku);
            if (await SkuExistsAsync(sku, excludeProductId: id))
            {
                throw new InvalidOperationException($"A different product already uses the SKU '{sku}'.");
            }

            product.Name = dto.Name.Trim();
            product.Sku = sku;
            product.Description = NormalizeDescription(dto.Description);
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.LastUpdatedDate = DateTime.UtcNow;

            await SaveChangesWithConflictHandlingAsync(sku);

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return MapToResponse(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> RestockAsync(int id, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return null;
            }

            if (quantity > int.MaxValue - product.StockQuantity)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Restocking by this quantity would exceed the maximum stock level.");
            }

            product.StockQuantity += quantity;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Restocked product {ProductName} (ID: {ProductId}) by {Quantity} units.",
                product.Name, product.Id, quantity);

            return MapToResponse(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> when the category does not exist.
    /// </summary>
    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            throw new ArgumentException($"Category {categoryId} does not exist.", "CategoryId");
        }
    }

    /// <summary>
    /// Checks whether a SKU is already used (case-insensitive), optionally excluding a product.
    /// </summary>
    private Task<bool> SkuExistsAsync(string normalizedSku, int? excludeProductId)
    {
        // Compare against the upper-cased stored value so legacy rows saved before
        // normalization are still matched case-insensitively.
        return _context.Products.AnyAsync(p =>
            p.Sku.ToUpper() == normalizedSku &&
            (!excludeProductId.HasValue || p.Id != excludeProductId.Value));
    }

    /// <summary>
    /// Saves changes, translating database constraint violations (e.g. a concurrent
    /// duplicate SKU insert) into an <see cref="InvalidOperationException"/>.
    /// </summary>
    private async Task SaveChangesWithConflictHandlingAsync(string sku)
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database update conflict while saving product with SKU {Sku}.", sku);
            throw new InvalidOperationException(SkuConflictMessage, ex);
        }
    }

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedDate = product.CreatedDate,
            LastUpdatedDate = product.LastUpdatedDate
        };
    }
}
