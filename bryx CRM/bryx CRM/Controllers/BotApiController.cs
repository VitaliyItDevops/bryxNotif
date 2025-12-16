using bryx_CRM.Data;
using bryx_CRM.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bryx_CRM.Controllers;

[ApiController]
[Route("api/bot")]
[AllowAnonymous]
public class BotApiController : ControllerBase
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<BotApiController> _logger;

    public BotApiController(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<BotApiController> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var total = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Subcategory,
                    p.PurchasePrice,
                    p.SalePrice,
                    p.Status,
                    p.Supplier,
                    p.Color,
                    p.IsFavorite,
                    p.IsDefective,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                products
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении товаров");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var product = await context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Subcategory,
                    p.PurchasePrice,
                    p.SalePrice,
                    p.PlannedPrice,
                    p.Status,
                    p.Supplier,
                    p.Color,
                    p.Buyer,
                    p.IsFavorite,
                    p.IsDefective,
                    p.AdditionalService,
                    p.ArrivalDate,
                    p.SaleDate,
                    p.DeliveryDate,
                    p.CreatedAt,
                    p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { error = "Товар не найден" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении товара {ProductId}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = context.Sales.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.SaleDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.SaleDate <= toDate.Value);
            }

            var total = await query.CountAsync();
            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.Buyer,
                    s.SaleDate,
                    s.TotalAmount,
                    s.Status,
                    s.TTN,
                    s.SoldThrough,
                    s.AdditionalService,
                    ProductCount = s.Products.Count
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                sales
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении продаж");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("sales/{id}")]
    public async Task<IActionResult> GetSale(int id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var sale = await context.Sales
                .Include(s => s.Products)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Buyer,
                    s.SaleDate,
                    s.TotalAmount,
                    s.Status,
                    s.TTN,
                    s.SoldThrough,
                    s.AdditionalService,
                    s.CreatedAt,
                    s.UpdatedAt,
                    Products = s.Products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Category,
                        p.SalePrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (sale == null)
            {
                return NotFound(new { error = "Продажа не найдена" });
            }

            return Ok(sale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении продажи {SaleId}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var totalProducts = await context.Products.CountAsync();
            var inStockProducts = await context.Products.CountAsync(p => p.Status == "В наличии");
            var soldProducts = await context.Products.CountAsync(p => p.Status == "Продано");
            var expectedProducts = await context.Products.CountAsync(p => p.Status == "Ожидается");

            var totalSales = await context.Sales.CountAsync();
            var totalSalesAmount = await context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var todaySales = await context.Sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .CountAsync();
            var todaySalesAmount = await context.Sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var categories = await context.Products
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(new
            {
                products = new
                {
                    total = totalProducts,
                    inStock = inStockProducts,
                    sold = soldProducts,
                    expected = expectedProducts
                },
                sales = new
                {
                    total = totalSales,
                    totalAmount = totalSalesAmount,
                    today = new
                    {
                        count = todaySales,
                        amount = todaySalesAmount
                    }
                },
                categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var categories = await context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    ProductCount = context.Products.Count(p => p.Category == c.Name)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении категорий");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpPost("sales/{id}/ship")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MarkSaleAsShipped(int id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var sale = await context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound(new { error = "Продажа не найдена" });
            }

            if (sale.Status != "Ожидает")
            {
                return BadRequest(new { error = $"Невозможно отправить продажу со статусом '{sale.Status}'" });
            }

            // Обновляем статус продажи
            sale.Status = "Отправлено";
            sale.UpdatedAt = DateTime.UtcNow;

            // Обновляем статус всех товаров в продаже
            var products = await context.Products
                .Where(p => p.SaleId == id)
                .ToListAsync();

            foreach (var product in products)
            {
                product.Status = "Отправлено";
                product.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Sale {SaleId} marked as shipped", id);

            return Ok(new
            {
                success = true,
                message = "Продажа отмечена как отправленная",
                saleId = id,
                newStatus = sale.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении статуса продажи {SaleId}", id);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("users")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetAllowedUsers()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var allowedUsers = await context.BotUsers
                .Where(u => u.IsActive)
                .Select(u => u.ChatId)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} allowed bot users", allowedUsers.Count);

            return Ok(new
            {
                allowedUsers = allowedUsers,
                count = allowedUsers.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей бота");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}
