using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.QrCode;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.ParkingMembershipSpecification;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed class AddTicketCommandHandler : ICommandHandler<AddTicketCommand, AddTicketCommandResponse>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<PricingScheme> _pricingSchema;

        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IByteVerificationService _byteVerificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly ILogger<AddTicketCommandHandler> _logger;
        private readonly IGenericRepository<ParkingMemberships> _parkingMembershipsRepository;

        public AddTicketCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<ParkingMemberships> parkingMembershipsRepository,
            IQRCodeService qrCodeService,
            IHttpContextAccessor httpContextAccessor, IByteVerificationService byteVerificationService,
            ITokenReader tokenReader,
            IGenericRepository<PricingScheme> pricingSchema,
            ILogger<AddTicketCommandHandler> logger, IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository)
        {
            _logger = logger;
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository;
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _pricingSchema = pricingSchema ?? throw new ArgumentNullException(nameof(pricingSchema));
            _parkingMembershipsRepository = parkingMembershipsRepository ?? throw new ArgumentNullException(nameof(parkingMembershipsRepository));
        }

        public async Task<Result<AddTicketCommandResponse>> Handle(AddTicketCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetParkingSystemConfigurationForUpdateSpecification();
            var configuration = await _parkingSystemConfigurationRepository
                .FirstOrDefaultWithSpecAsync(spec, cancellationToken);

            if (configuration is null)
            {
                return Result<AddTicketCommandResponse>.Fail(new
                    Error("Configuration not found", "Configuration not found", ErrorType.NotFound));
            }
            var tokenInfo = _httpContextAccessor!.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
            {
                return Result<AddTicketCommandResponse>.Fail(new Error("GateId not found", "GateId not found", ErrorType.NotFound));
            }
            if (request.IsSubscriber == true)
            {
                var specCard = new GetCardSummaryByIdSpec(request.CardNumber);

                var Subscriber = await _parkingMembershipsRepository.FirstOrDefaultWithSpecAsync(specCard, cancellationToken);
                if (Subscriber == null)
                {
                    return Result<AddTicketCommandResponse>.
                        Fail(new Error("Card not found", "Card not found", ErrorType.NotFound));
                }
                var ticketEntity = Ticket.Create(DateTime.Now, 0,
                   tokenInfo.GateId.Value, tokenInfo.UserId.Value);
                ticketEntity.SetSubscriber(Subscriber.NationalId, Subscriber.VehicleNumber);
                await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
                await _ticketRepository.SaveChangesAsync(cancellationToken);
                var response = new AddTicketCommandResponse
                {
                    CreatedAt = ticketEntity.StartDate,
                    Price = ticketEntity.Price,
                    QrCode = [],
                    TicketId = ticketEntity.Id,
                    TicketInfo = ticketEntity.VehicleNumber
                };
                return Result<AddTicketCommandResponse>.Ok(response);
            }
            if (configuration.PriceType == PriceType.Enter)
            {
                var pricingSchema = await _pricingSchema.GetByIdAsync(configuration.PricingSchemaId!.Value, cancellationToken);
                var ticketEntity = Ticket.Create(DateTime.Now, pricingSchema!.Salary,
                    tokenInfo.GateId.Value, tokenInfo.UserId.Value

              );
                if (!string.IsNullOrEmpty(request.VehicleNumber)) ticketEntity.SetVehicleNumber(request.VehicleNumber);
                await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
                await _ticketRepository.SaveChangesAsync(cancellationToken);
                var Tbyte5 = _byteVerificationService.GenerateComplexByte5FromGuid(ticketEntity.UniqueGuidPart);
                var combinedBytes = ticketEntity.UniqueGuidPart.Concat(new byte[] { Tbyte5 }).ToArray();
                var encryptedData = Convert.ToBase64String(combinedBytes);
                var TqrCode = _qrCodeService.GenerateQRCode(encryptedData);
                var response = new AddTicketCommandResponse
                {
                    CreatedAt = ticketEntity.StartDate,
                    Price = ticketEntity.Price,
                    QrCode = TqrCode,
                    TicketId = ticketEntity.Id,
                    TicketInfo = ticketEntity.VehicleNumber
                };
                return Result<AddTicketCommandResponse>.Ok(response);
            }
            else
            {
                var ticketEntity = Ticket.Create(DateTime.Now, 0,
                       tokenInfo.GateId.Value, tokenInfo.UserId.Value

                 );
                await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
                await _ticketRepository.SaveChangesAsync(cancellationToken);
                var Tbyte5 = _byteVerificationService.GenerateComplexByte5FromGuid(ticketEntity.UniqueGuidPart);
                var combinedBytes = ticketEntity.UniqueGuidPart.Concat(new byte[] { Tbyte5 }).ToArray();
                var encryptedData = Convert.ToBase64String(combinedBytes);
                var TqrCode = _qrCodeService.GenerateQRCode(encryptedData);
                var response = new AddTicketCommandResponse
                {
                    CreatedAt = ticketEntity.StartDate,
                    Price = ticketEntity.Price,
                    QrCode = TqrCode,
                    TicketId = ticketEntity.Id,
                    TicketInfo = ticketEntity.VehicleNumber
                };
                return Result<AddTicketCommandResponse>.Ok(response);
            }
        }
    }
}