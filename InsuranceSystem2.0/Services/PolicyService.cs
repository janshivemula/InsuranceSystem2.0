using AutoMapper;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Policy;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;

namespace InsuranceSystem2._0.Services.Implementations
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPolicyPlanRepository _planRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(
            IPolicyRepository policyRepository,
            ICustomerRepository customerRepository,
            IPolicyPlanRepository planRepository,
            IMapper mapper,
            ILogger<PolicyService> logger)
        {
            _policyRepository = policyRepository;
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Get all policies
        public async Task<PagedResponse<PolicyResponseDto>> GetAllPoliciesAsync(PolicyQueryDto query)
        {
            var result = await _policyRepository.GetAllAsync(query);

            _logger.LogInformation(
                "Retrieved policy list. Page={Page}, PageSize={PageSize}",
                query.PageNumber,
                query.PageSize);

            foreach (var policy in result.Items)
            {
                await UpdatePolicyStatusAsync(policy);
            }

            var response = _mapper.Map<List<PolicyResponseDto>>(result.Items);

            for (int i = 0; i < response.Count; i++)
            {
                PopulatePaymentDetails(response[i], result.Items.ElementAt(i));
            }

            return new PagedResponse<PolicyResponseDto>
            {
                Records = response,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = (int)Math.Ceiling((double)result.TotalRecords / query.PageSize),
                IsLastPage = query.PageNumber >=
                             (int)Math.Ceiling((double)result.TotalRecords / query.PageSize),
                SortField = query.SortBy,
                SortDirection = query.SortDirection
            };
        }
        // Get policies by customer
        public async Task<IEnumerable<PolicyResponseDto>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            var policies = (await _policyRepository
                .GetPoliciesByCustomerIdAsync(customerId))
                .ToList();

            foreach (var policy in policies)
            {
                await UpdatePolicyStatusAsync(policy);
            }

            var response = _mapper.Map<List<PolicyResponseDto>>(policies);

            for (int i = 0; i < response.Count; i++)
            {
                PopulatePaymentDetails(response[i], policies[i]);
            }

            return response;
        }

        // Get active policies of customer
        public async Task<IEnumerable<PolicyResponseDto>> GetActivePoliciesByCustomerIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            if (!customer.IsActive)
                throw new BadRequestException("Customer is inactive.");

            var policies = (await _policyRepository
                .GetPoliciesByCustomerIdAsync(customerId))
                .ToList();

            foreach (var policy in policies)
            {
                await UpdatePolicyStatusAsync(policy);
            }

            var activePolicies = policies
                .Where(p => p.PolicyStatus == PolicyStatus.Active)
                .ToList();

            var response = _mapper.Map<List<PolicyResponseDto>>(activePolicies);

            for (int i = 0; i < response.Count; i++)
            {
                PopulatePaymentDetails(response[i], activePolicies[i]);
            }

            return response;
        }

        // Get my policies
        public async Task<IEnumerable<PolicyResponseDto>> GetMyPoliciesAsync(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            var policies = (await _policyRepository
                .GetPoliciesByUserIdAsync(userId))
                .ToList();

            foreach (var policy in policies)
            {
                await UpdatePolicyStatusAsync(policy);
            }

            var response = _mapper.Map<List<PolicyResponseDto>>(policies);

            for (int i = 0; i < response.Count; i++)
            {
                PopulatePaymentDetails(response[i], policies[i]);
            }

            return response;
        }

        // Get all active policies
        public async Task<IEnumerable<PolicyResponseDto>> GetActivePoliciesAsync()
        {
            var policies = (await _policyRepository
                .GetActivePoliciesAsync())
                .ToList();

            foreach (var policy in policies)
            {
                await UpdatePolicyStatusAsync(policy);
            }

            var response = _mapper.Map<List<PolicyResponseDto>>(policies);

            for (int i = 0; i < response.Count; i++)
            {
                PopulatePaymentDetails(response[i], policies[i]);
            }

            return response;
        }

        // Get policy by Id
        public async Task<PolicyResponseDto> GetPolicyByIdAsync(
            int policyId,
            int loggedInUserId,
            string role)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);

            if (policy == null)
                throw new NotFoundException("Policy not found.");

            _logger.LogInformation(
                "Policy {PolicyId} retrieved.",
                policy.PolicyId);

            await UpdatePolicyStatusAsync(policy);

            if (role == "Customer" &&
                policy.Customer.UserId != loggedInUserId)
            {
                throw new UnauthorizedAccessException(
                    "You can only view your own policy.");
            }

            var response = _mapper.Map<PolicyResponseDto>(policy);

            PopulatePaymentDetails(response, policy);

            return response;
        }

        // Get policy by Policy Number
        public async Task<PolicyResponseDto> GetPolicyByPolicyNumberAsync(
            string policyNumber,
            int loggedInUserId,
            string role)
        {
            if (string.IsNullOrWhiteSpace(policyNumber))
                throw new BadRequestException("Policy number is required.");

            var policy = await _policyRepository
                .GetByPolicyNumberAsync(policyNumber.Trim());

            if (policy == null)
                throw new NotFoundException("Policy not found.");

            await UpdatePolicyStatusAsync(policy);

            if (role == "Customer" &&
                policy.Customer.UserId != loggedInUserId)
            {
                throw new UnauthorizedAccessException(
                    "You can only view your own policy.");
            }

            var response = _mapper.Map<PolicyResponseDto>(policy);

            PopulatePaymentDetails(response, policy);

            return response;
        }
        // Customer purchases policy
        public async Task<PolicyResponseDto> PurchasePolicyAsync(
            int customerUserId,
            PurchasePolicyRequestDto requestDto)
        {
            var customer = await _customerRepository.GetByUserIdAsync(customerUserId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            if (!customer.IsActive)
                throw new BadRequestException("Inactive customer cannot purchase policies.");

            if (customer.DateOfBirth == default ||
                string.IsNullOrWhiteSpace(customer.Address) ||
                string.IsNullOrWhiteSpace(customer.City) ||
                string.IsNullOrWhiteSpace(customer.State) ||
                string.IsNullOrWhiteSpace(customer.PinCode) ||
                string.IsNullOrWhiteSpace(customer.NomineeName) ||
                string.IsNullOrWhiteSpace(customer.NomineeRelation))
            {
                throw new BadRequestException(
                    "Please complete your customer profile before purchasing a policy.");
            }

            var plan = await _planRepository.GetByIdAsync(requestDto.PlanId);

            if (plan == null)
                throw new NotFoundException("Policy plan not found.");

            if (!plan.IsActive)
                throw new BadRequestException("Policy plan is inactive.");

            if (!plan.InsuranceProduct.IsActive)
                throw new BadRequestException("Insurance product is inactive.");

            var existingPolicies = await _policyRepository
                .GetPoliciesByCustomerIdAsync(customer.CustomerId);

            if (existingPolicies.Any(p =>
                p.PlanId == requestDto.PlanId &&
                p.PolicyStatus != PolicyStatus.Cancelled &&
                p.PolicyStatus != PolicyStatus.Expired))
            {
                throw new ConflictException("Customer already owns this policy.");
            }

            if (requestDto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadRequestException("Start date cannot be in the past.");

            var policy = new Policy
            {
                PolicyNumber = await GeneratePolicyNumberAsync(),
                CustomerId = customer.CustomerId,
                InsuranceProductId = plan.InsuranceProductId,
                PlanId = plan.PlanId,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.StartDate.AddYears(plan.DurationInYears),
                PolicyStatus = PolicyStatus.PendingPayment,
                TotalPremiumPaid = 0,
                LastPaymentDate = null,
                NextDueDate = requestDto.StartDate.ToDateTime(TimeOnly.MinValue),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _policyRepository.AddAsync(policy);
            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Policy {PolicyNumber} purchased by Customer {CustomerId}.",
                policy.PolicyNumber,
                customer.CustomerId);

            var createdPolicy = await _policyRepository.GetByIdAsync(policy.PolicyId);

            await UpdatePolicyStatusAsync(createdPolicy!);

            var response = _mapper.Map<PolicyResponseDto>(createdPolicy);

            PopulatePaymentDetails(response, createdPolicy!);

            return response;
        }

        // Internal Staff issues policy
        public async Task<PolicyResponseDto> IssuePolicyByInternalStaffAsync(
            IssuePolicyRequestDto requestDto)
        {
            var customer = await _customerRepository.GetByIdAsync(requestDto.CustomerId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            if (!customer.IsActive)
                throw new BadRequestException("Inactive customer cannot receive policies.");

            if (customer.DateOfBirth == default ||
                string.IsNullOrWhiteSpace(customer.Address) ||
                string.IsNullOrWhiteSpace(customer.City) ||
                string.IsNullOrWhiteSpace(customer.State) ||
                string.IsNullOrWhiteSpace(customer.PinCode) ||
                string.IsNullOrWhiteSpace(customer.NomineeName) ||
                string.IsNullOrWhiteSpace(customer.NomineeRelation))
            {
                throw new BadRequestException("Customer profile is incomplete.");
            }

            var plan = await _planRepository.GetByIdAsync(requestDto.PlanId);

            if (plan == null)
                throw new NotFoundException("Policy plan not found.");

            if (!plan.IsActive)
                throw new BadRequestException("Policy plan is inactive.");

            if (!plan.InsuranceProduct.IsActive)
                throw new BadRequestException("Insurance product is inactive.");

            var existingPolicies = await _policyRepository
                .GetPoliciesByCustomerIdAsync(customer.CustomerId);

            if (existingPolicies.Any(p =>
                p.PlanId == requestDto.PlanId &&
                p.PolicyStatus != PolicyStatus.Cancelled &&
                p.PolicyStatus != PolicyStatus.Expired))
            {
                throw new ConflictException("Customer already owns this policy.");
            }

            if (requestDto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadRequestException("Start date cannot be in the past.");

            var policy = new Policy
            {
                PolicyNumber = await GeneratePolicyNumberAsync(),
                CustomerId = customer.CustomerId,
                InsuranceProductId = plan.InsuranceProductId,
                PlanId = plan.PlanId,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.StartDate.AddYears(plan.DurationInYears),
                PolicyStatus = PolicyStatus.PendingPayment,
                TotalPremiumPaid = 0,
                LastPaymentDate = null,
                NextDueDate = requestDto.StartDate.ToDateTime(TimeOnly.MinValue),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _policyRepository.AddAsync(policy);
            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Policy {PolicyNumber} issued to Customer {CustomerId}.",
                policy.PolicyNumber,
                customer.CustomerId);

            var createdPolicy = await _policyRepository.GetByIdAsync(policy.PolicyId);

            await UpdatePolicyStatusAsync(createdPolicy!);

            var response = _mapper.Map<PolicyResponseDto>(createdPolicy);

            PopulatePaymentDetails(response, createdPolicy!);

            return response;
        }
        public async Task<PolicyResponseDto> CancelPolicyAsync(int policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);

            if (policy == null)
                throw new NotFoundException("Policy not found.");

            if (policy.PolicyStatus == PolicyStatus.Cancelled)
                throw new BadRequestException("Policy is already cancelled.");

            if (policy.PolicyStatus == PolicyStatus.Expired)
                throw new BadRequestException("Expired policy cannot be cancelled.");

            policy.PolicyStatus = PolicyStatus.Cancelled;
            policy.UpdatedDate = DateTime.UtcNow;

            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Policy {PolicyNumber} cancelled.",
                policy.PolicyNumber);

            var response = _mapper.Map<PolicyResponseDto>(policy);

            PopulatePaymentDetails(response, policy);

            return response;
        }

        private async Task UpdatePolicyStatusAsync(Policy policy)
        {
            CalculatePaymentDetails(policy);

            bool isChanged = false;
            var today = DateTime.UtcNow.Date;

            if (policy.EndDate.ToDateTime(TimeOnly.MinValue).Date < today)
            {
                if (policy.PolicyStatus != PolicyStatus.Expired)
                {
                    policy.PolicyStatus = PolicyStatus.Expired;
                    isChanged = true;
                }
            }


            if (isChanged)
            {
                policy.UpdatedDate = DateTime.UtcNow;

                await _policyRepository.UpdateAsync(policy);
                await _policyRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Policy {PolicyNumber} status updated to {Status}.",
                    policy.PolicyNumber,
                    policy.PolicyStatus);
            }
        }

        private void CalculatePaymentDetails(Policy policy)
        {
            if (policy.Plan == null)
                return;


            var successfulPayments = policy.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Success)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();


            policy.LastPaymentDate = successfulPayments
                .Select(p => (DateTime?)p.PaymentDate)
                .FirstOrDefault();



            if (policy.Plan.PremiumType == PremiumType.OneTime)
            {
                policy.NextDueDate = null;
                return;
            }



            if (policy.LastPaymentDate.HasValue)
            {
                switch (policy.Plan.PremiumType)
                {
                    case PremiumType.Monthly:

                        policy.NextDueDate =
                            policy.LastPaymentDate.Value.AddMonths(1);

                        break;


                    case PremiumType.Quarterly:

                        policy.NextDueDate =
                            policy.LastPaymentDate.Value.AddMonths(3);

                        break;


                    case PremiumType.HalfYearly:

                        policy.NextDueDate =
                            policy.LastPaymentDate.Value.AddMonths(6);

                        break;


                    case PremiumType.Annual:

                        policy.NextDueDate =
                            policy.LastPaymentDate.Value.AddYears(1);

                        break;
                }
            }
            else
            {
                policy.NextDueDate =
                    policy.StartDate.ToDateTime(TimeOnly.MinValue);
            }
        }

        private void PopulatePaymentDetails(PolicyResponseDto dto, Policy policy)
        {
            CalculatePaymentDetails(policy);

            dto.LastPaymentDate = policy.LastPaymentDate;
            dto.NextDueDate = policy.NextDueDate;

            dto.IsPaymentDue =
                dto.NextDueDate.HasValue &&
                dto.NextDueDate.Value.Date <= DateTime.UtcNow.Date;

            dto.InstallmentsPaid =
                policy.Payments.Count(p => p.PaymentStatus == PaymentStatus.Success);

            dto.TotalPremiumPaid = policy.TotalPremiumPaid;

            if (policy.Plan != null)
            {
                switch (policy.Plan.PremiumType)
                {
                    case PremiumType.OneTime:
                        dto.TotalInstallments = 1;
                        break;

                    case PremiumType.Monthly:
                        dto.TotalInstallments =
                            policy.Plan.DurationInYears * 12;
                        break;

                    case PremiumType.Quarterly:
                        dto.TotalInstallments =
                            policy.Plan.DurationInYears * 4;
                        break;

                    case PremiumType.HalfYearly:
                        dto.TotalInstallments =
                            policy.Plan.DurationInYears * 2;
                        break;

                    case PremiumType.Annual:
                        dto.TotalInstallments =
                            policy.Plan.DurationInYears;
                        break;
                }

                // PremiumAmount is ONE installment amount
                dto.PremiumAmount = policy.Plan.PremiumAmount;

                // Total premium for the whole policy
                dto.TotalPremiumAmount =
                    policy.Plan.PremiumAmount *
                    dto.TotalInstallments;
            }

            dto.RemainingPremiumAmount =
                dto.TotalPremiumAmount -
                dto.TotalPremiumPaid;

            if (dto.RemainingPremiumAmount < 0)
            {
                dto.RemainingPremiumAmount = 0;
            }
        }
        private async Task<string> GeneratePolicyNumberAsync()
        {
            string policyNumber;
            bool exists;

            do
            {
                policyNumber = $"POL{DateTime.UtcNow:yyMMddHHmmss}";
                exists = await _policyRepository.GetByPolicyNumberAsync(policyNumber) != null;
            }
            while (exists);

            return policyNumber;
        }
    }
}