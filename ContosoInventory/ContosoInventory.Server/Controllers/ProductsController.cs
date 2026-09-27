using ContosoInventory.Server.Services;
using ContosoInventory.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoInventory.Server.Controllers;

/// <summary>
/// Manages inventory product operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <param name="logger">The logger.</param>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all products ordered by name, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category identifier to filter by.</param>
    /// <returns>A list of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts([FromQuery] int? categoryId)
    {
        _logger.LogInformation("Retrieving products (category filter: {CategoryId}).", categoryId);
        var products = await _productService.GetAllAsync(categoryId);
        return Ok(products);
    }

    /// <summary>
    /// Returns a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product details.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        _logger.LogInformation("Retrieving product with ID {ProductId}.", id);
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product '{ProductName}'.", dto.Name);

        try
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Replaces an existing product's editable fields.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with ID {ProductId}.", id);

        try
        {
            var product = await _productService.UpdateAsync(id, dto);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}.", id);
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Restocks a product by the specified quantity.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The restock data containing the quantity to add.</param>
    /// <returns>The updated product.</returns>
    [HttpPost("{id:int}/restock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restock([FromRoute] int id, [FromBody] RestockProductDto dto)
    {
        _logger.LogInformation("Restocking product with ID {ProductId} by {Quantity} units.", id, dto.Quantity);

        try
        {
            var product = await _productService.RestockAsync(id, dto.Quantity);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
