using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext db,
        ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductResponseDto>>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "Fetching products - Category: {Category}, Search: {Search}, Page: {Page}",
            category, search, page);

        var query = _db.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p =>
                p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search));

        var total = await query.CountAsync();

        var items = await query
    .OrderBy(p => p.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new ProductResponseDto(
        p.Id, p.Name, p.Description,
        p.Price, p.Stock, p.Category,
        p.IsActive, p.CreatedAt))
    .ToListAsync();

        _logger.LogInformation("Fetched {Count} products.", items.Count);

        return Ok(new ApiResponse<PagedResult<ProductResponseDto>>(
            true, "Products fetched.",
            new PagedResult<ProductResponseDto>(items, total, page, pageSize)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetById(int id)
    {
        _logger.LogInformation("Fetching product with ID: {Id}", id);

        var product = await _db.Products.FindAsync(id);

        if (product == null || !product.IsActive)
        {
            _logger.LogWarning("Product not found with ID: {Id}", id);
            return NotFound(new ApiResponse<ProductResponseDto>(
                false, "Product not found."));
        }

        return Ok(new ApiResponse<ProductResponseDto>(
            true, "Product found.",
            new ProductResponseDto(
                product.Id, product.Name, product.Description,
                product.Price, product.Stock, product.Category,
                product.IsActive, product.CreatedAt)));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Create(
        CreateProductDto dto)
    {
        _logger.LogInformation("Creating product: {Name}", dto.Name);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product created with ID: {Id}", product.Id);

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new ApiResponse<ProductResponseDto>(
                true, "Product created.",
                new ProductResponseDto(
                    product.Id, product.Name, product.Description,
                    product.Price, product.Stock, product.Category,
                    product.IsActive, product.CreatedAt)));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Update(
        int id, UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with ID: {Id}", id);

        var product = await _db.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product not found with ID: {Id}", id);
            return NotFound(new ApiResponse<ProductResponseDto>(
                false, "Product not found."));
        }

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.Category != null) product.Category = dto.Category;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Product updated with ID: {Id}", id);

        return Ok(new ApiResponse<ProductResponseDto>(
            true, "Product updated.",
            new ProductResponseDto(
                product.Id, product.Name, product.Description,
                product.Price, product.Stock, product.Category,
                product.IsActive, product.CreatedAt)));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        _logger.LogInformation("Deleting product with ID: {Id}", id);

        var product = await _db.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product not found with ID: {Id}", id);
            return NotFound(new ApiResponse<object>(
                false, "Product not found."));
        }

        product.IsActive = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product soft deleted with ID: {Id}", id);

        return Ok(new ApiResponse<object>(true, "Product deleted."));
    }
}