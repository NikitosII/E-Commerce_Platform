using ECommerce.Application.DTOs.Customer;
using ECommerce.Application.Interfaces;
using ECommerce.Common.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        PhoneNumber = c.PhoneNumber,
        Address = c.Address
    };

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        return customer is null || customer.IsDeleted ? null : MapToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);
        return customer is null || customer.IsDeleted ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Customers.GetByEmailAsync(dto.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException(string.Format(ErrorMessages.AlreadyExists, $"Customer with email '{dto.Email}'"));

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created: {CustomerId} ({Email})", customer.Id, customer.Email);
        return MapToDto(customer);
    }

    public async Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Customer), id));

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
