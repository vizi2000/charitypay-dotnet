using AutoMapper;
using Microsoft.Extensions.Logging;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.Common.Models;
using CharityPay.Application.DTOs.Payment;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Services;

/// <summary>
/// Implementation of payment processing service.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<InitiatePaymentResponse> InitiatePaymentAsync(
        InitiatePaymentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(request.OrganizationId, cancellationToken);
        
        if (organization == null || organization.Status != OrganizationStatus.Active)
        {
            throw new InvalidOperationException("Organization not found or not approved");
        }
        
        // Create payment record
        var payment = Payment.Create(
            request.Amount,
            request.PaymentMethod,
            request.OrganizationId,
            request.DonorName,
            request.DonorEmail,
            request.DonorPhone
        );
        
        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // TODO: Integrate with Fiserv payment gateway
        var fiservOrderId = $"ORDER_{payment.Id:N}";
        var paymentUrl = $"https://test.fiserv.com/payment/{fiservOrderId}";
        
        payment.UpdateFiservDetails(fiservOrderId, null, paymentUrl);
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Payment {PaymentId} initiated for organization {OrganizationId} with amount {Amount}", 
            payment.Id, organization.Id, request.Amount);
        
        return new InitiatePaymentResponse
        {
            PaymentId = payment.Id,
            PaymentUrl = paymentUrl,
            OrderId = fiservOrderId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        };
    }
    
    public async Task<PaymentDto?> GetPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
        
        if (payment == null)
        {
            return null;
        }
        
        return _mapper.Map<PaymentDto>(payment);
    }
    
    public async Task<bool> CancelPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
        
        if (payment == null || payment.Status != PaymentStatus.Pending)
        {
            return false;
        }
        
        payment.Cancel();
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Payment {PaymentId} cancelled", paymentId);
        
        return true;
    }
    
    public async Task<PaginatedResponse<PaymentDto>> GetOrganizationPaymentsAsync(
        Guid organizationId, 
        int page = 1, 
        int pageSize = 10,
        PaymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var (payments, totalCount) = await _unitOfWork.Payments
            .GetByOrganizationPaginatedAsync(organizationId, page, pageSize, cancellationToken);
        
        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);
        
        return new PaginatedResponse<PaymentDto>
        {
            Items = paymentDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    
    public async Task ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Fiserv webhook processing
        // 1. Verify webhook signature
        // 2. Parse payload
        // 3. Update payment status
        // 4. Update organization collected amount if completed
        
        _logger.LogInformation("Processing payment webhook");
        
        await Task.CompletedTask;
    }
    
    public async Task<(decimal TotalAmount, int TotalCount, int CompletedCount)> GetPaymentStatisticsAsync(
        Guid organizationId, 
        DateTime fromDate, 
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Payments.GetStatisticsAsync(organizationId, fromDate, toDate, cancellationToken);
    }
}