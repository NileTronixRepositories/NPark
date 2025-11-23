using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.QrCode;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed class AddTicketCommandHandler : ICommandHandler<AddTicketCommand, byte[]>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<PricingScheme> _pricingSchema;

        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IByteVerificationService _byteVerificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly ILogger<AddTicketCommandHandler> _logger;

        public AddTicketCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
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

        }

        public async Task<Result<byte[]>> Handle(AddTicketCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetParkingSystemConfigurationForUpdateSpecification();
            var configuration = await _parkingSystemConfigurationRepository
                .FirstOrDefaultWithSpecAsync(spec, cancellationToken);

            if (configuration is null)
            {
                return Result<byte[]>.Fail(new
                    Error("Configuration not found", "Configuration not found", ErrorType.NotFound));
            }
            var tokenInfo = _httpContextAccessor!.HttpContext?.ReadToken(_tokenReader);
            if (tokenInfo is null || !tokenInfo.GateId.HasValue || !tokenInfo.UserId.HasValue)
            {
                return Result<byte[]>.Fail(new Error("GateId not found", "GateId not found", ErrorType.NotFound));
            }
            if (configuration.PriceType == PriceType.Enter)
            {
                var pricingSchema = await _pricingSchema.GetByIdAsync(configuration.PricingSchemaId!.Value, cancellationToken);
                var ticketEntity = Ticket.Create(DateTime.Now, pricingSchema!.Salary,
                    tokenInfo.GateId.Value, tokenInfo.UserId.Value

              );
                await _ticketRepository.AddAsync(ticketEntity, cancellationToken);
                await _ticketRepository.SaveChangesAsync(cancellationToken);
                var Tbyte5 = _byteVerificationService.GenerateComplexByte5FromGuid(ticketEntity.UniqueGuidPart);
                var combinedBytes = ticketEntity.UniqueGuidPart.Concat(new byte[] { Tbyte5 }).ToArray();
                var encryptedData = Convert.ToBase64String(combinedBytes);
                var TqrCode = _qrCodeService.GenerateQRCode(encryptedData);
                return Result<byte[]>.Ok(TqrCode);
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
                return Result<byte[]>.Ok(TqrCode);
            }
        }
    }
}