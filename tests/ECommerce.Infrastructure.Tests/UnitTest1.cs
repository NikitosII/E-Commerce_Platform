using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Tests;

public class ProductRepositoryTests
{
    private static ECommerceDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ECommerceDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProduct()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var category = new Category { Name = "Books" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repo = new ProductRepository(context);
        var product = new Product
        {
            Name = "Clean Code",
            Price = 29.99m,
            StockQuantity = 5,
            CategoryId = category.Id
        };

        // Act
        await repo.AddAsync(product);
        await context.SaveChangesAsync();

        // Assert
        var saved = await context.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Clean Code");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenNotPresent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context);

        // Act
        var exists = await repo.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }
}
