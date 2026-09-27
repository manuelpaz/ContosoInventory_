using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;
using ContosoInventory.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContosoInventory.Server.Services;

public class ProductService : IProductService
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(InventoryContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        try
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products.");
            throw;
        }
    }

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

    public async Task<IEnumerable<ProductResponseDto>> GetByCategoryIdAsync(int categoryId)
    {
        try
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}.", categoryId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductResponseDto>> GetLowStockAsync()
    {
        try
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.ReorderLevel)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return products.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low-stock products.");
            throw;
        }
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            var sku = dto.Sku.Trim();
            var skuExists = await _context.Products.AnyAsync(p => p.Sku == sku);
            if (skuExists)
            {
                throw new InvalidOperationException("A product with this SKU already exists.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Sku = sku,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ReorderLevel = dto.ReorderLevel,
                IsActive = true,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var created = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (created is null)
            {
                throw new InvalidOperationException("Product could not be created.");
            }

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return MapToResponse(created);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product '{ProductName}'.", dto.Name);
            throw;
        }
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        if (dto is null)
        {
            return null;
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

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                product.Name = dto.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                product.Description = dto.Description.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var normalizedSku = dto.Sku.Trim();
                var duplicateExists = await _context.Products
                    .AnyAsync(p => p.Id != id && p.Sku == normalizedSku);

                if (duplicateExists)
                {
                    throw new InvalidOperationException("A different product already uses this SKU.");
                }

                product.Sku = normalizedSku;
            }

            if (dto.Price.HasValue)
            {
                product.Price = dto.Price.Value;
            }

            if (dto.StockQuantity.HasValue)
            {
                product.StockQuantity = dto.StockQuantity.Value;
            }

            if (dto.ReorderLevel.HasValue)
            {
                product.ReorderLevel = dto.ReorderLevel.Value;
            }

            if (dto.IsActive.HasValue)
            {
                product.IsActive = dto.IsActive.Value;
            }

            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!categoryExists)
                {
                    throw new KeyNotFoundException("Category not found.");
                }

                product.CategoryId = dto.CategoryId.Value;
            }

            product.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw;
        }
    }

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

    public async Task<ProductResponseDto?> ToggleActiveAsync(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return null;
            }

            product.IsActive = !product.IsActive;
            product.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product toggled: {ProductName} (ID: {ProductId}) is now {Status}.",
                product.Name, product.Id, product.IsActive ? "active" : "inactive");

            return MapToResponse(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling active status for product with ID {ProductId}.", id);
            throw;
        }
    }

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

            product.StockQuantity += quantity;
            product.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Restocked product {ProductName} (ID: {ProductId}) by {Quantity} units.",
                product.Name, product.Id, quantity);

            return MapToResponse(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw;
        }
    }

    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedDate = product.CreatedDate,
            LastModifiedDate = product.LastModifiedDate
        };
    }
}
