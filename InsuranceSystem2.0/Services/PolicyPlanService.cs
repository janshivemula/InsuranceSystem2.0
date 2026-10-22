using AutoMapper;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Plan;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;

public class PolicyPlanService : IPolicyPlanService
{
    private readonly IPolicyPlanRepository _planRepository;
    private readonly IInsuranceProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PolicyPlanService> _logger;

    public PolicyPlanService(
        IPolicyPlanRepository planRepository,
        IInsuranceProductRepository productRepository,
        IMapper mapper,
        ILogger<PolicyPlanService> logger)
    {
        _planRepository = planRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // Get all plans
    public async Task<PagedResponse<PlanResponseDto>> GetAllPlansAsync(PolicyPlanQueryDto query)
    {
        var pagedPlans = await _planRepository.GetAllAsync(query);

        _logger.LogInformation("Retrieved policy plans. PageNumber: {PageNumber}, PageSize: {PageSize}",
            query.PageNumber, query.PageSize);

        return new PagedResponse<PlanResponseDto>
        {
            Records = _mapper.Map<IEnumerable<PlanResponseDto>>(pagedPlans.Records),
            CurrentPage = pagedPlans.CurrentPage,
            PageSize = pagedPlans.PageSize,
            TotalRecords = pagedPlans.TotalRecords,
            TotalPages = pagedPlans.TotalPages,
            IsLastPage = pagedPlans.IsLastPage,
            SortField = pagedPlans.SortField,
            SortDirection = pagedPlans.SortDirection
        };
    }

    // Get active plans
    public async Task<IEnumerable<PlanResponseDto>> GetActivePlansAsync()
    {
        var plans = await _planRepository.GetActivePlansAsync();

        _logger.LogInformation("Retrieved active policy plans.");

        return _mapper.Map<IEnumerable<PlanResponseDto>>(plans);
    }

    // Get plans by product
    public async Task<IEnumerable<PlanResponseDto>> GetPlansByProductIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
            throw new NotFoundException($"Insurance Product with Id {productId} not found.");

        var plans = await _planRepository.GetByProductIdAsync(productId);

        _logger.LogInformation("Retrieved policy plans for Product Id {ProductId}.", productId);

        return _mapper.Map<IEnumerable<PlanResponseDto>>(plans);
    }

    // Get active plans by product
    public async Task<IEnumerable<PlanResponseDto>> GetActivePlansByProductIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
            throw new NotFoundException($"Insurance Product with Id {productId} not found.");

        if (!product.IsActive)
            throw new BadRequestException("Insurance Product is inactive.");

        var plans = await _planRepository.GetActivePlansByProductIdAsync(productId);

        _logger.LogInformation("Retrieved active policy plans for Product Id {ProductId}.", productId);

        return _mapper.Map<IEnumerable<PlanResponseDto>>(plans);
    }

    // Get plan by Id
    public async Task<PlanResponseDto?> GetPlanByIdAsync(int id)
    {
        var plan = await _planRepository.GetByIdAsync(id);

        _logger.LogInformation("Retrieved policy plan with Id {PlanId}.", id);

        if (plan == null)
            return null;

        return _mapper.Map<PlanResponseDto>(plan);
    }

    // Create new plan
    public async Task<PlanResponseDto> CreatePlanAsync(PlanRequestDto requestDto)
    {
        ValidatePlan(requestDto);

        var product = await _productRepository.GetByIdAsync(requestDto.InsuranceProductId);

        if (product == null)
            throw new NotFoundException($"Insurance Product with Id {requestDto.InsuranceProductId} not found.");

        if (!product.IsActive)
            throw new BadRequestException("Cannot create a plan for an inactive insurance product.");

        string planName = requestDto.PlanName.Trim();

        var existingPlan = await _planRepository.GetByNameAsync(planName);

        if (existingPlan != null)
            throw new ConflictException("Plan name already exists.");

        var plan = new PolicyPlan
        {
            InsuranceProductId = requestDto.InsuranceProductId,
            PlanName = planName,
            CoverageAmount = requestDto.CoverageAmount,
            PremiumAmount = requestDto.PremiumAmount,
            PremiumType = requestDto.PremiumType,
            DurationInYears = requestDto.DurationInYears,
            TermsAndConditions = requestDto.TermsAndConditions.Trim(),
            IsActive = requestDto.IsActive,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        await _planRepository.AddAsync(plan);
        await _planRepository.SaveChangesAsync();

        _logger.LogInformation("Policy plan '{PlanName}' created successfully with Id {PlanId}.",
            plan.PlanName, plan.PlanId);

        var createdPlan = await _planRepository.GetByIdAsync(plan.PlanId);

        if (createdPlan == null)
            throw new BadRequestException("Plan creation failed.");

        return _mapper.Map<PlanResponseDto>(createdPlan);
    }

    // Update existing plan
    public async Task UpdatePlanAsync(int id, PlanRequestDto requestDto)
    {
        ValidatePlan(requestDto);

        var plan = await _planRepository.GetByIdAsync(id);

        if (plan == null)
            throw new NotFoundException($"Policy Plan with Id {id} not found.");



        var product = await _productRepository.GetByIdAsync(requestDto.InsuranceProductId);

        if (product == null)
            throw new NotFoundException($"Insurance Product with Id {requestDto.InsuranceProductId} not found.");

        if (!product.IsActive)
            throw new BadRequestException("Cannot assign an inactive insurance product.");

        string planName = requestDto.PlanName.Trim();

        var existingPlan = await _planRepository.GetByNameAsync(planName);

        if (existingPlan != null && existingPlan.PlanId != id)
            throw new ConflictException("Another plan with the same name already exists.");

        plan.InsuranceProductId = requestDto.InsuranceProductId;
        plan.PlanName = planName;
        plan.CoverageAmount = requestDto.CoverageAmount;
        plan.PremiumAmount = requestDto.PremiumAmount;
        plan.PremiumType = requestDto.PremiumType;
        plan.DurationInYears = requestDto.DurationInYears;
        plan.TermsAndConditions = requestDto.TermsAndConditions.Trim();
        plan.IsActive = requestDto.IsActive;
        plan.UpdatedDate = DateTime.UtcNow;

        await _planRepository.UpdateAsync(plan);
        await _planRepository.SaveChangesAsync();

        _logger.LogInformation("Policy plan with Id {PlanId} updated successfully.", id);
    }

    // Soft delete plan
    public async Task SoftDeletePlanAsync(int id)
    {
        var plan = await _planRepository.GetByIdAsync(id);

        if (plan == null)
            throw new NotFoundException($"Policy Plan with Id {id} not found.");

        if (!plan.IsActive)
            throw new BadRequestException("Policy Plan is already inactive.");

        await _planRepository.SoftDeleteAsync(plan);
        await _planRepository.SaveChangesAsync();

        _logger.LogInformation("Policy plan with Id {PlanId} deactivated successfully.", id);
    }

    private static void ValidatePlan(PlanRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PlanName))
            throw new BadRequestException("Plan name is required.");

        if (dto.PlanName.Trim().Length < 3)
            throw new BadRequestException("Plan name must contain at least 3 characters.");

        if (dto.PlanName.Trim().Length > 100)
            throw new BadRequestException("Plan name cannot exceed 100 characters.");

        if (dto.CoverageAmount <= 0)
            throw new BadRequestException("Coverage amount must be greater than zero.");

        if (dto.PremiumAmount <= 0)
            throw new BadRequestException("Premium amount must be greater than zero.");

        if (dto.CoverageAmount <= dto.PremiumAmount)
            throw new BadRequestException("Coverage amount must be greater than premium amount.");

        if (dto.DurationInYears <= 0)
            throw new BadRequestException("Duration must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.TermsAndConditions))
            throw new BadRequestException("Terms and Conditions are required.");

        if (dto.TermsAndConditions.Trim().Length > 1000)
            throw new BadRequestException("Terms and Conditions cannot exceed 1000 characters.");

        if (!Enum.IsDefined(typeof(PremiumType), dto.PremiumType))
            throw new BadRequestException("Invalid premium type.");
    }
}