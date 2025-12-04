using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.QrCode;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed class AddTicketCommandHandler
     : ICommandHandler<AddTicketCommand, AddTicketCommandResponse>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<PricingScheme> _pricingSchemaRepository;
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IByteVerificationService _byteVerificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly ILogger<AddTicketCommandHandler> _logger;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public AddTicketCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<ParkingMemberships> parkingMembershipsRepository,
            IQRCodeService qrCodeService,
            IHttpContextAccessor httpContextAccessor,
            IByteVerificationService byteVerificationService,
            ITokenReader tokenReader,
            IGenericRepository<PricingScheme> pricingSchemaRepository,
            ILogger<AddTicketCommandHandler> logger,
            IRealtimeNotifier realtimeNotifier,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _parkingMembershipsRepository = parkingMembershipsRepository ?? throw new ArgumentNullException(nameof(parkingMembershipsRepository));
            _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _pricingSchemaRepository = pricingSchemaRepository ?? throw new ArgumentNullException(nameof(pricingSchemaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));

            _realtimeNotifier = realtimeNotifier ?? throw new ArgumentNullException(nameof(realtimeNotifier));
        }

        public async Task<Result<AddTicketCommandResponse>> Handle(
            AddTicketCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Load configuration
                // ---------------------------
                var configSpec = new GetParkingSystemConfigurationForUpdateSpecification();
                var configuration = await _parkingSystemConfigurationRepository
                    .FirstOrDefaultWithSpecAsync(configSpec, cancellationToken);

                if (configuration is null)
                {
                    _logger.LogWarning("Parking system configuration not found while adding ticket.");
                    return Result<AddTicketCommandResponse>.Fail(
                        new Error(
                            Code: "Configuration.NotFound",
                            Message: ErrorMessage.Configuration_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 2) Read token (GateId + UserId)
                // ---------------------------
                var tokenInfo = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
                if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
                {
                    _logger.LogWarning("Token info missing GateId/UserId while adding ticket.");
                    return Result<AddTicketCommandResponse>.Fail(
                        new Error(
                            Code: "Token.GateOrUser.NotFound",
                            Message: ErrorMessage.TokenInfo_Missing,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 3) Route by subscriber / normal ticket
                // ---------------------------
                if (request.IsSubscriber)
                {
                    return await HandleSubscriberTicketAsync(
                        request,
                        configuration,
                        tokenInfo.GateId.Value,
                        tokenInfo.UserId.Value,
                        cancellationToken);
                }

                return await HandleNormalTicketAsync(
                    request,
                    configuration,
                    tokenInfo.GateId.Value,
                    tokenInfo.UserId.Value,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("AddTicket operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding ticket.");
                return Result<AddTicketCommandResponse>.Fail(
                    new Error(
                        Code: "Ticket.Add.Unexpected",
                        Message: ErrorMessage.TicketAdd_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Subscriber path
        // --------------------------------------------------------
        private async Task<Result<AddTicketCommandResponse>> HandleSubscriberTicketAsync(
            AddTicketCommand request,
            ParkingSystemConfiguration configuration,
            Guid gateId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            // 1) Get subscriber by card number
            var cardSpec = new GetCardSummaryByIdSpec(request.CardNumber);
            var subscriber = await _parkingMembershipsRepository
                .FirstOrDefaultWithSpecAsync(cardSpec, cancellationToken);

            if (subscriber is null)
            {
                _logger.LogWarning("Subscriber card not found. CardNumber: {CardNumber}", request.CardNumber);
                return Result<AddTicketCommandResponse>.Fail(
                    new Error(
                        Code: "Subscriber.Card.NotFound",
                        Message: ErrorMessage.CardNumber_Require,
                        Type: ErrorType.NotFound));
            }

            // 2) Validate subscriber validity period
            var now = DateTime.Now;
            if (now < subscriber.CreatedAt || now >= subscriber.EndDate)
            {
                _logger.LogWarning("Invalid subscriber card date. CardNumber: {CardNumber}", request.CardNumber);
                return Result<AddTicketCommandResponse>.Fail(
                    new Error(
                        Code: "Subscriber.Card.InvalidDate",
                        Message: ErrorMessage.Subscriber_Card_InvalidDate,
                        Type: ErrorType.Validation));
            }

            // 3) Create ticket with subscriber data (no QR in this scenario)
            var ticketEntity = Ticket.Create(
                startDate: now,
                price: 0m,
                gateId: gateId,
                userId: userId);

            ticketEntity.SetSubscriber(subscriber.NationalId, subscriber.VehicleNumber, subscriber.CardNumber);

            await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
            await _ticketRepository.SaveChangesAsync(cancellationToken);

            var response = BuildResponse(ticketEntity, qrCode: Array.Empty<byte>());

            return Result<AddTicketCommandResponse>.Ok(response);
        }

        // --------------------------------------------------------
        // Normal ticket path (non-subscriber)
        // --------------------------------------------------------
        private async Task<Result<AddTicketCommandResponse>> HandleNormalTicketAsync(
            AddTicketCommand request,
            ParkingSystemConfiguration configuration,
            Guid gateId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.Now; // أو DateTime.UtcNow حسب نظامك

            Ticket ticketEntity;

            // PriceType = Enter → نحسب السعر وقت الدخول
            if (configuration.PriceType == PriceType.Enter)
            {
                if (!configuration.PricingSchemaId.HasValue)
                {
                    _logger.LogWarning("Configuration has PriceType=Enter but no PricingSchemaId.");
                    return Result<AddTicketCommandResponse>.Fail(
                        new Error(
                            Code: "Configuration.PricingSchema.Missing",
                            Message: "Pricing schema is not configured for enter price type.",
                            Type: ErrorType.Validation));
                }

                var pricingScheme = await _pricingSchemaRepository
                    .GetByIdAsync(configuration.PricingSchemaId.Value, cancellationToken);

                if (pricingScheme is null)
                {
                    _logger.LogWarning("Pricing scheme not found. Id: {PricingSchemaId}",
                        configuration.PricingSchemaId.Value);

                    return Result<AddTicketCommandResponse>.Fail(
                        new Error(
                            Code: "PricingSchema.NotFound",
                            Message: ErrorMessage.PricingSchema_NotFound,
                            Type: ErrorType.NotFound));
                }

                ticketEntity = Ticket.Create(
                    startDate: now,
                    price: pricingScheme.Salary,
                    gateId: gateId,
                    userId: userId);
            }
            else
            {
                // PriceType != Enter → السعر يتحسب لاحقاً (خروج مثلاً)
                ticketEntity = Ticket.Create(
                    startDate: now,
                    price: 0m,
                    gateId: gateId,
                    userId: userId);
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleNumber))
            {
                ticketEntity.SetVehicleNumber(request.VehicleNumber);
            }

            await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
            await _ticketRepository.SaveChangesAsync(cancellationToken);

            await NotifyDashboardTicketAddedAsync(
    ticketEntity,
    isSubscriber: true,
    cancellationToken);
            var qrCode = GenerateTicketQrCode(ticketEntity);
            var response = BuildResponse(ticketEntity, qrCode);

            return Result<AddTicketCommandResponse>.Ok(response);
        }

        private async Task NotifyDashboardTicketAddedAsync(
            Ticket ticketEntity,
            bool isSubscriber,
            CancellationToken cancellationToken)
        {
            try
            {
                var payload = new TicketAddedNotification
                {
                    TicketId = ticketEntity.Id,
                    GateId = ticketEntity.GateId,        // assuming Ticket has GateId property
                    StartDate = ticketEntity.StartDate,
                    Price = ticketEntity.Price,
                    IsSubscriber = isSubscriber,
                    VehicleNumber = ticketEntity.VehicleNumber
                };

                // channel name عام "tickets:added"
                await _realtimeNotifier.NotifyTicketAddedAsync(new TicketAddedNotification
                {
                    TicketId = ticketEntity.Id,
                    GateId = ticketEntity.GateId,
                    StartDate = ticketEntity.StartDate,
                    Price = ticketEntity.Price,
                    IsSubscriber = isSubscriber,
                    VehicleNumber = ticketEntity.VehicleNumber
                });
            }
            catch (Exception ex)
            {
                // مفيش فشل في العملية الأساسية لو SignalR وقع
                _logger.LogError(ex,
                    "Failed to send realtime notification for ticket {TicketId}",
                    ticketEntity.Id);
            }
        }

        // --------------------------------------------------------
        // Helper: Generate QR Code using UniqueGuidPart + Byte5
        // --------------------------------------------------------
        private byte[] GenerateTicketQrCode(Ticket ticketEntity)
        {
            var bytesGenerated = Array.Empty<byte>();
            do
            {
                var byte5 = _byteVerificationService.GenerateComplexByte5FromGuid(ticketEntity.UniqueGuidPart);
                var combinedBytes = ticketEntity.UniqueGuidPart.Concat(new[] { byte5 }).ToArray();
                var encryptedData = Convert.ToBase64String(combinedBytes);
                bytesGenerated = combinedBytes;
            } while (bytesGenerated.Length != 5);
            // byte5 based on UniqueGuidPart

            return _qrCodeService.GenerateQRCode(bytesGenerated);
        }

        // --------------------------------------------------------
        // Helper: Build response DTO
        // --------------------------------------------------------
        private static AddTicketCommandResponse BuildResponse(Ticket ticketEntity, byte[] qrCode)
        {
            return new AddTicketCommandResponse
            {
                CreatedAt = ticketEntity.StartDate,
                Price = ticketEntity.Price,
                QrCode = qrCode ?? Array.Empty<byte>(),
                TicketId = ticketEntity.Id,
                TicketInfo = BitConverter.ToString(ticketEntity.UniqueGuidPart).Replace("-", ""),
            };
        }
    }
}