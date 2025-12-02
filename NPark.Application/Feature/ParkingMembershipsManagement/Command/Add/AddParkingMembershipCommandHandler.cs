using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.FileNames;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.ParkingMembershipsManagement.Command.Add
{
    public sealed class AddParkingMembershipCommandHandler
        : ICommandHandler<AddParkingMembershipCommand>
    {
        private readonly IGenericRepository<ParkingMemberships> _parkingRepository;
        private readonly IGenericRepository<PricingScheme> _pricingRepository;
        private readonly IMediaService _mediaService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AddParkingMembershipCommandHandler> _logger;

        public AddParkingMembershipCommandHandler(
            IGenericRepository<PricingScheme> pricingRepository,
            IGenericRepository<ParkingMemberships> parkingRepository,
            IMediaService mediaService,
            IAuditLogger auditLogger,
            ILogger<AddParkingMembershipCommandHandler> logger)
        {
            _pricingRepository = pricingRepository ?? throw new ArgumentNullException(nameof(pricingRepository));
            _parkingRepository = parkingRepository ?? throw new ArgumentNullException(nameof(parkingRepository));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(
            AddParkingMembershipCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Ensure PricingScheme exists
                // ---------------------------
                var pricingEntity = await _pricingRepository
                    .GetByIdAsync(request.PricingSchemeId, cancellationToken);

                if (pricingEntity is null)
                {
                    _logger.LogWarning(
                        "AddParkingMembership failed: PricingScheme not found. PricingSchemeId: {PricingSchemeId}",
                        request.PricingSchemeId);

                    return Result.Fail(
                        new Error(
                            Code: "PricingScheme.NotFound",
                            Message: ErrorMessage.PricingSchema_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 2) Check unique Phone
                // ---------------------------
                var phoneExists = await _parkingRepository
                    .IsExistAsync(x => x.Phone == request.Phone, cancellationToken);

                if (phoneExists)
                {
                    _logger.LogWarning(
                        "AddParkingMembership failed: phone already exists. Phone: {Phone}",
                        request.Phone);

                    return Result.Fail(
                        new Error(
                            Code: "Membership.Phone.Exists",
                            Message: ErrorMessage.MembershipPhoneExists,
                            Type: ErrorType.Conflict));
                }

                // ---------------------------
                // 3) Check unique NationalId
                // ---------------------------
                var nationalIdExists = await _parkingRepository
                    .IsExistAsync(x => x.NationalId == request.NationalId, cancellationToken);

                if (nationalIdExists)
                {
                    _logger.LogWarning(
                        "AddParkingMembership failed: NationalId already exists. NationalId: {NationalId}",
                        request.NationalId);

                    return Result.Fail(
                        new Error(
                            Code: "Membership.NationalId.Exists",
                            Message: ErrorMessage.Unique_NationalId,
                            Type: ErrorType.Conflict));
                }

                // ---------------------------
                // 4) Calculate end date based on pricing duration
                // ---------------------------
                var now = DateTime.Now;
                var endDate = now;

                switch (pricingEntity.DurationType)
                {
                    case DurationType.Days:
                        endDate = now.AddDays(pricingEntity.TotalDays ?? 0);
                        break;

                    case DurationType.Hours:
                        endDate = now.AddHours(pricingEntity.TotalHours ?? 0);
                        break;

                    case DurationType.Years:
                        endDate = now.AddYears(1);
                        break;

                    default:
                        // لو DurationType مش معروف، نخليه نفس اليوم كـ fallback
                        _logger.LogWarning(
                            "Unknown DurationType for PricingScheme. PricingSchemeId: {PricingSchemeId}, DurationType: {DurationType}",
                            pricingEntity.Id, pricingEntity.DurationType);
                        break;
                }

                // ---------------------------
                // 5) Create membership entity
                // ---------------------------
                var membership = ParkingMemberships.Create(
                    name: request.Name,
                    phone: request.Phone,
                    nationalId: request.NationalId,
                    vehicleNumber: request.VehicleNumber,
                    cardNumber: request.CardNumber,
                    pricingSchemeId: request.PricingSchemeId,
                    createdAt: now,
                    endDate: endDate);

                // ---------------------------
                // 6) Save vehicle images (if any)
                // ---------------------------
                if (request.VehicleImage is { Count: > 0 })
                {
                    foreach (var file in request.VehicleImage)
                    {
                        var filePath = await _mediaService
                            .SaveAsync(file, FileNames.ParkingMemberships);

                        membership.AddAttachment(filePath);
                    }
                }

                // ---------------------------
                // 7) Persist membership
                // ---------------------------
                await _parkingRepository.AddAsync(membership, cancellationToken);
                await _parkingRepository.SaveChangesAsync(cancellationToken);

                // ---------------------------
                // 8) Audit (Safe)
                // ---------------------------
                await SafeAuditMembershipCreatedAsync(
                    membership,
                    pricingEntity,
                    cancellationToken);

                _logger.LogInformation(
                    "Parking membership created successfully. MembershipId: {MembershipId}, NationalId: {NationalId}",
                    membership.Id, membership.NationalId);

                return Result.Ok();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("AddParkingMembership operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding parking membership.");

                return Result.Fail(
                    new Error(
                        Code: "Membership.Add.Unexpected",
                        Message: "حدث خطأ غير متوقع أثناء إضافة الاشتراك، برجاء المحاولة لاحقًا (Unexpected error occurred while adding parking membership, please try again later).",
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Safe audit logging
        // --------------------------------------------------------
        private async Task SafeAuditMembershipCreatedAsync(
            ParkingMemberships membership,
            PricingScheme pricingScheme,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "ParkingMembershipCreated",
                        EventCategory: "ParkingMembership",
                        IsSuccess: true,
                        StatusCode: StatusCodes.Status201Created,
                        UserId: null,
                        GateId: null,
                        Extra: new
                        {
                            MembershipId = membership.Id,
                            membership.Name,
                            membership.Phone,
                            membership.NationalId,
                            membership.VehicleNumber,
                            membership.CardNumber,
                            membership.CreatedOnUtc,
                            membership.EndDate,
                            PricingSchemeId = pricingScheme.Id,
                            pricingScheme.Salary,
                            CreatedAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for ParkingMembershipCreated. MembershipId: {MembershipId}",
                    membership.Id);
            }
        }
    }
}