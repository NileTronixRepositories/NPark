using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.QrCode;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPark.Application.Abstraction.Security;
using NPark.Application.Options;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.TicketsManagement.Command.Add
{
    public sealed class AddTicketCommandHandler : ICommandHandler<AddTicketCommand, byte[]>
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IByteVerificationService _byteVerificationService;
        private readonly SalaryConfig _option;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AddTicketCommandHandler> _logger;

        public AddTicketCommandHandler(IGenericRepository<Ticket> ticketRepository,
            IQRCodeService qrCodeService, IOptionsMonitor<SalaryConfig> option,
            IHttpContextAccessor httpContextAccessor, IByteVerificationService byteVerificationService,
            ILogger<AddTicketCommandHandler> logger)
        {
            _logger = logger;
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
            _option = option.CurrentValue ?? throw new ArgumentNullException(nameof(option));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
        }

        public async Task<Result<byte[]>> Handle(AddTicketCommand request, CancellationToken cancellationToken)
        {
            var endTime = DateTime.UtcNow.Add(_option.AllowedTime);
            var entity = Ticket.Create(DateTime.UtcNow, endTime, _option.Salary);
            await _ticketRepository.AddAsync(entity, cancellationToken);
            _logger.LogWarning("UniqueGuidPart Before Save: " + BitConverter.ToString(entity.UniqueGuidPart));

            await _ticketRepository.SaveChangesAsync(cancellationToken);
            var uniqueGuidPart = entity.UniqueGuidPart;
            var byte5 = _byteVerificationService.GenerateComplexByte5FromGuid(uniqueGuidPart);
            var combinedBytes = uniqueGuidPart.Concat(new byte[] { byte5 }).ToArray();
            var encryptedData = Convert.ToBase64String(combinedBytes);

            var qrCode = _qrCodeService.GenerateQRCode(encryptedData);

            return Result<byte[]>.Ok(qrCode);
        }

        //private string GetUri(string id)
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    var ipHost = $"{request.Scheme}://{request.Host.Value}";

        //    var fullUrl = $"{ipHost}?id={id}";

        //    return fullUrl;
        //}
    }
}