using AutoMapper;
using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.DTOs.Customer;
using InsuranceSystem2._0.DTOs.Plan;
using InsuranceSystem2._0.DTOs.Policy;
using InsuranceSystem2._0.DTOs.PremiumPayent;
using InsuranceSystem2._0.DTOs.Product;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Models;

namespace InsuranceSystem2._0.MappingProfile
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // =========================
            // USER
            // =========================

            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<RegisterRequestDto, User>();

            CreateMap<CreateAdminRequestDto, User>();

            CreateMap<CreateInternalStaffRequestDto, User>();


            // =========================
            // CUSTOMER
            // =========================

            CreateMap<CustomerRequestDto, Customer>()
                .ForMember(dest => dest.DateOfBirth,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.DateOfBirth)));

            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.MobileNumber,
                    opt => opt.MapFrom(src => src.User.MobileNumber))
                .ForMember(dest => dest.DateOfBirth,
                    opt => opt.MapFrom(src => src.DateOfBirth.ToDateTime(TimeOnly.MinValue)));


            // =========================
            // INSURANCE PRODUCT
            // =========================

            CreateMap<ProductRequestDto, InsuranceProduct>()
                .ForMember(dest => dest.ProductType,
                    opt => opt.MapFrom(src => Enum.Parse<ProductType>(src.ProductType, true)));

            CreateMap<InsuranceProduct, ProductResponseDto>()
                .ForMember(dest => dest.ProductType,
                    opt => opt.MapFrom(src => src.ProductType.ToString()));


            // =========================
            // POLICY PLAN
            // =========================

            CreateMap<PlanRequestDto, PolicyPlan>();

            CreateMap<PolicyPlan, PlanResponseDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.InsuranceProduct.ProductName))
                .ForMember(dest => dest.ProductType,
                    opt => opt.MapFrom(src => src.InsuranceProduct.ProductType.ToString()));


            // =========================
            // POLICY
            // =========================

            CreateMap<Policy, PolicyResponseDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.User.FullName))
                .ForMember(dest => dest.CustomerEmail,
                    opt => opt.MapFrom(src => src.Customer.User.Email))
                .ForMember(dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Plan.PlanName))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.InsuranceProduct.ProductName))
                .ForMember(dest => dest.ProductType,
                    opt => opt.MapFrom(src => src.InsuranceProduct.ProductType))
                .ForMember(dest => dest.CoverageAmount,
                    opt => opt.MapFrom(src => src.Plan.CoverageAmount))
                .ForMember(dest => dest.PremiumAmount,
                    opt => opt.MapFrom(src => src.Plan.PremiumAmount))
                .ForMember(dest => dest.PremiumType,
                    opt => opt.MapFrom(src => src.Plan.PremiumType))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src => src.EndDate.ToDateTime(TimeOnly.MinValue)));


            // =========================
            // PREMIUM PAYMENT
            // =========================

            CreateMap<PremiumPayment, PremiumPaymentResponseDto>()

    .ForMember(dest => dest.PolicyId,
        opt => opt.MapFrom(src => src.PolicyId))

    .ForMember(dest => dest.CustomerId,
        opt => opt.MapFrom(src => src.CustomerId))

    .ForMember(dest => dest.CustomerName,
        opt => opt.MapFrom(src => src.Customer.User.FullName))

    .ForMember(dest => dest.PolicyNumber,
        opt => opt.MapFrom(src => src.Policy.PolicyNumber))

    .ForMember(dest => dest.TotalPremiumPaid,
        opt => opt.MapFrom(src => src.Policy.TotalPremiumPaid))

    .ForMember(dest => dest.NextDueDate,
        opt => opt.MapFrom(src => src.Policy.NextDueDate))

    .ForMember(dest => dest.IsPaymentDue,
        opt => opt.MapFrom(src =>
            src.Policy.NextDueDate != null &&
            src.Policy.NextDueDate <= DateTime.UtcNow));


            // =========================
            // CLAIM DOCUMENT
            // =========================


            CreateMap<ClaimDocumentRequestDto, ClaimDocument>()
                .ForMember(dest => dest.DocumentReference,
                    opt => opt.Ignore());

            CreateMap<ClaimDocument, ClaimDocumentResponseDto>()
                .ForMember(dest => dest.FilePath,
                    opt => opt.MapFrom(src => src.DocumentReference));


            // =========================
            // CLAIM STATUS HISTORY
            // =========================

            CreateMap<ClaimStatusHistory, ClaimStatusHistoryResponseDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.HistoryId))
                .ForMember(dest => dest.UpdatedByUserId,
                    opt => opt.MapFrom(src => src.UpdatedBy))
                .ForMember(dest => dest.UpdatedByUserName,
                    opt => opt.MapFrom(src => src.User.FullName));


            // =========================
            // CLAIM
            // =========================



            CreateMap<ClaimRequestDto, Claim>()
    .ForMember(
        dest => dest.IncidentDate,
        opt => opt.MapFrom(src =>
            DateOnly.FromDateTime(src.IncidentDate)));

            CreateMap<Claim, ClaimResponseDto>()
                .ForMember(
                    dest => dest.PolicyNumber,
                    opt => opt.MapFrom(src => src.Policy.PolicyNumber))
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src =>
                        src.Policy.Plan.InsuranceProduct.ProductName))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src =>
                        src.Policy.Plan.PlanName))
                .ForMember(
                    dest => dest.CustomerName,
                    opt => opt.MapFrom(src =>
                        src.Customer.User.FullName))
                .ForMember(
                    dest => dest.IncidentDate,
                    opt => opt.MapFrom(src =>
                        src.IncidentDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(
                    dest => dest.ReviewedByName,
                    opt => opt.MapFrom(src =>
                        src.ReviewedByUser == null
                            ? null
                            : src.ReviewedByUser.FullName))
                .ForMember(
                    dest => dest.DecidedByName,
                    opt => opt.MapFrom(src =>
                        src.DecidedByUser == null
                            ? null
                            : src.DecidedByUser.FullName))
                .ForMember(
                    dest => dest.Documents,
                    opt => opt.MapFrom(src => src.ClaimDocuments))
                .ForMember(
                    dest => dest.StatusHistory,
                    opt => opt.MapFrom(src => src.ClaimStatusHistories));

        }
    }
}
