using AutoMapper;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.PremiumPayent;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;

public class PremiumPaymentService : IPremiumPaymentService
{
    private readonly IPremiumPaymentRepository _premiumPaymentRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PremiumPaymentService> _logger;

    public PremiumPaymentService(
        IPremiumPaymentRepository premiumPaymentRepository,
        IPolicyRepository policyRepository,
        IMapper mapper,
        ILogger<PremiumPaymentService> logger)
    {
        _premiumPaymentRepository = premiumPaymentRepository;
        _policyRepository = policyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // Get all payments
    public async Task<PagedResponse<PremiumPaymentResponseDto>> GetAllPaymentsAsync(PaginationRequestDto paginationDto)
    {
        _logger.LogInformation(
            "Retrieving premium payments. Page: {Page}, Size: {Size}",
            paginationDto.PageNumber,
            paginationDto.PageSize);

        var pagedPayments =
            await _premiumPaymentRepository.GetAllAsync(paginationDto);

        return new PagedResponse<PremiumPaymentResponseDto>
        {
            Records = _mapper.Map<IEnumerable<PremiumPaymentResponseDto>>(pagedPayments.Records),
            CurrentPage = pagedPayments.CurrentPage,
            PageSize = pagedPayments.PageSize,
            TotalRecords = pagedPayments.TotalRecords,
            TotalPages = pagedPayments.TotalPages,
            IsLastPage = pagedPayments.IsLastPage,
            SortField = pagedPayments.SortField,
            SortDirection = pagedPayments.SortDirection
        };
    }

    // Get payments by Policy Id
    public async Task<PagedResponse<PremiumPaymentResponseDto>> GetPaymentsByPolicyIdAsync(
        int policyId,
        PaginationRequestDto paginationDto)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId);

        if (policy == null)
            throw new NotFoundException("Policy not found.");

        var pagedPayments =
            await _premiumPaymentRepository.GetPaymentsByPolicyIdAsync(
                policyId,
                paginationDto);

        return new PagedResponse<PremiumPaymentResponseDto>
        {
            Records = _mapper.Map<IEnumerable<PremiumPaymentResponseDto>>(pagedPayments.Records),
            CurrentPage = pagedPayments.CurrentPage,
            PageSize = pagedPayments.PageSize,
            TotalRecords = pagedPayments.TotalRecords,
            TotalPages = pagedPayments.TotalPages,
            IsLastPage = pagedPayments.IsLastPage,
            SortField = pagedPayments.SortField,
            SortDirection = pagedPayments.SortDirection
        };
    }
    // Get payments by Customer Id
    public async Task<PagedResponse<PremiumPaymentResponseDto>> GetPaymentsByCustomerIdAsync(
        int customerId,
        int userId,
        string role,
        PaginationRequestDto paginationDto)
    {
        var policies = await _policyRepository
            .GetPoliciesByCustomerIdAsync(customerId);

        if (!policies.Any())
        {
            return new PagedResponse<PremiumPaymentResponseDto>
            {
                Records = Enumerable.Empty<PremiumPaymentResponseDto>(),
                CurrentPage = paginationDto.PageNumber,
                PageSize = paginationDto.PageSize,
                TotalRecords = 0,
                TotalPages = 0,
                IsLastPage = true,
                SortField = paginationDto.SortBy,
                SortDirection = paginationDto.SortDirection
            };
        }

        // Customer can see only own payment history
        if (role == "Customer")
        {
            var customerPolicy = policies.First();

            if (customerPolicy.Customer.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can view only your own payment history.");
            }
        }

        var pagedPayments =
            await _premiumPaymentRepository
                .GetPaymentsByCustomerIdAsync(customerId, paginationDto);

        return new PagedResponse<PremiumPaymentResponseDto>
        {
            Records = _mapper.Map<IEnumerable<PremiumPaymentResponseDto>>
                        (pagedPayments.Records),

            CurrentPage = pagedPayments.CurrentPage,
            PageSize = pagedPayments.PageSize,
            TotalRecords = pagedPayments.TotalRecords,
            TotalPages = pagedPayments.TotalPages,
            IsLastPage = pagedPayments.IsLastPage,
            SortField = pagedPayments.SortField,
            SortDirection = pagedPayments.SortDirection
        };
    }


    // Get payment by Id
    public async Task<PremiumPaymentResponseDto?> GetPaymentByIdAsync(int id)
    {
        _logger.LogInformation(
            "Retrieving payment with ID {PaymentId}",
            id);

        var payment =
            await _premiumPaymentRepository.GetByIdAsync(id);

        if (payment == null)
            return null;

        return _mapper.Map<PremiumPaymentResponseDto>(payment);
    }
    // Make Premium Payment
    public async Task<PremiumPaymentResponseDto> MakePaymentAsync(
    PremiumPaymentRequestDto requestDto,
    int userId,
    string role)
    {
        var policy = await _policyRepository
            .GetByIdAsync(requestDto.PolicyId);

        if (policy == null)
            throw new NotFoundException("Policy not found.");


        // Customer can pay only own policy
        if (role == "Customer")
        {
            if (policy.Customer.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can make payment only for your own policy.");
            }
        }


        // Customer validation
        if (!policy.Customer.IsActive)
            throw new BadRequestException(
                "Customer account is inactive.");


        // Plan validation
        if (!policy.Plan.IsActive)
            throw new BadRequestException(
                "Policy plan is inactive.");


        // Product validation
        if (!policy.InsuranceProduct.IsActive)
            throw new BadRequestException(
                "Insurance product is inactive.");


        // Policy status validation
        if (policy.PolicyStatus == PolicyStatus.Cancelled)
            throw new BadRequestException(
                "Cancelled policies cannot accept payments.");


        if (policy.PolicyStatus == PolicyStatus.Expired)
            throw new BadRequestException(
                "Expired policies cannot accept payments.");


        // CHANGED: the old "cannot pay before policy start date" check was removed.
        // The first premium can now be paid in advance. Further installments are
        // not accepted until the policy has actually started.
        if (policy.PolicyStatus == PolicyStatus.Scheduled)
            throw new BadRequestException(
                "Initial premium is already paid. The policy will become Active on its start date.");



        // Calculate installment amount

        decimal installmentAmount = policy.Plan.PremiumAmount;

        // Calculate total premium amount

        decimal totalPremiumAmount = policy.Plan.PremiumAmount;

        switch (policy.Plan.PremiumType)
        {
            case PremiumType.Monthly:
                totalPremiumAmount *= policy.Plan.DurationInYears * 12;
                break;

            case PremiumType.Quarterly:
                totalPremiumAmount *= policy.Plan.DurationInYears * 4;
                break;

            case PremiumType.HalfYearly:
                totalPremiumAmount *= policy.Plan.DurationInYears * 2;
                break;

            case PremiumType.Annual:
                totalPremiumAmount *= policy.Plan.DurationInYears;
                break;

            case PremiumType.OneTime:
                break;
        }

        // Prevent over payment

        var remainingAmount =
            totalPremiumAmount -
            policy.TotalPremiumPaid;

        if (remainingAmount <= 0)
        {
            throw new BadRequestException(
                "Premium amount is already fully paid.");
        }

        if (installmentAmount > remainingAmount)
        {
            installmentAmount = remainingAmount;
        }


        // CHANGED: transaction reference is now generated by the system
        // (the user no longer sends it).
        string transactionReference =
            await GenerateTransactionReferenceAsync();


        // Create payment

        var payment = new PremiumPayment
        {
            CustomerId = policy.CustomerId,

            PolicyId = policy.PolicyId,

            Amount = installmentAmount,

            PaymentDate = DateTime.UtcNow,

            PaymentMode = requestDto.PaymentMode,

            TransactionReference = transactionReference,

            PaymentStatus = PaymentStatus.Success,

            CreatedDate = DateTime.UtcNow
        };



        await _premiumPaymentRepository.AddAsync(payment);



        // Update policy payment details

        policy.TotalPremiumPaid += installmentAmount;


        policy.LastPaymentDate =
            DateTime.UtcNow;



        // CHANGED: after the first premium is paid, the policy becomes Active only if
        // the start date has arrived. Otherwise it is Scheduled until the start date.
        if (policy.PolicyStatus == PolicyStatus.PendingPayment)
        {
            policy.PolicyStatus =
                policy.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow)
                    ? PolicyStatus.Active
                    : PolicyStatus.Scheduled;
        }




        // CHANGED: next due date is calculated from the policy START DATE and the
        // number of installments paid (including this one), not from today.
        if (policy.Plan.PremiumType == PremiumType.OneTime)
        {
            policy.NextDueDate = null;
        }
        else
        {
            int monthsPerInstallment = policy.Plan.PremiumType switch
            {
                PremiumType.Monthly => 1,
                PremiumType.Quarterly => 3,
                PremiumType.HalfYearly => 6,
                PremiumType.Annual => 12,
                _ => 0
            };

            int installmentsPaid =
                policy.Payments.Count(p => p.PaymentStatus == PaymentStatus.Success) + 1;

            policy.NextDueDate = policy.StartDate
                .ToDateTime(TimeOnly.MinValue)
                .AddMonths(monthsPerInstallment * installmentsPaid);
        }



        policy.UpdatedDate =
            DateTime.UtcNow;



        await _policyRepository.UpdateAsync(policy);


        await _premiumPaymentRepository.SaveChangesAsync();



        return _mapper.Map<PremiumPaymentResponseDto>(payment);
    }

    // NEW: generates a unique transaction reference, e.g. TXN20261026143015A1B2C3D4
    private async Task<string> GenerateTransactionReferenceAsync()
    {
        string reference;

        do
        {
            reference =
                $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}" +
                $"{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
        while (await _premiumPaymentRepository
                   .GetByTransactionReferenceAsync(reference) != null);

        return reference;
    }
}
