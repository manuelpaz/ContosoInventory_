using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync();
    Task<ProductResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductResponseDto>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<ProductResponseDto>> GetLowStockAsync();
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<ProductResponseDto?> ToggleActiveAsync(int id);
    Task<ProductResponseDto?> RestockAsync(int id, int quantity);
}
