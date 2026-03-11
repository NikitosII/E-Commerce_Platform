using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<ProductService>> _loggerMock = new();

    private ProductService CreateSut() =>
        new(_unitOfWorkMock.Object, _cacheMock.Object, _loggerMock.Object);

    [Fact]
    public async Task CreateProduct_ShouldReturnDto_WhenValid()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Price = 999.99m,
            StockQuantity = 10,
            CategoryId = category.Id
        };

        _unitOfWorkMock.Setup(u => u.Categories.ExistsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateProductAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Laptop");
        result.Price.Should().Be(999.99m);
        result.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _cacheMock.Setup(c => c.GetOrSetAsync<ProductDto?>(
            It.IsAny<string>(),
            It.IsAny<Func<Task<ProductDto?>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<ProductDto?>>, TimeSpan?, CancellationToken>(
                async (_, factory, __, ___) => await factory());

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.GetProductByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProduct_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.DeleteProductAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
