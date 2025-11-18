using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;
using System.Text.Json;

namespace NPark.Application.Feature.DespanserManagement.Command.ExitAck
{
    public class ExitAckCommandHandler : ICommandHandler<ExitAckCommand, string>
    {
        private readonly ILogger<ExitAckCommandHandler> _logger;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IByteVerificationService _byteVerificationService;
        private readonly ISendProtocol _sendProtocolService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExitAckCommandHandler(ILogger<ExitAckCommandHandler> logger,
            IGenericRepository<Ticket> ticketRepository, IByteVerificationService byteVerificationService,
            ISendProtocol sendProtocolService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _sendProtocolService = sendProtocolService ?? throw new ArgumentNullException(nameof(sendProtocolService));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
        }

        public async Task<Result<string>> Handle(ExitAckCommand request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("ExitAckCommandHandler.Handle {0}", JsonSerializer.Serialize(request));

            if (request.Type == "Q")
            {
                byte[] payload = BuildPacket(false);
                string payloadHex = BitConverter.ToString(payload).Replace("-", "");
                byte[] bytes5;
                try
                {
                    bytes5 = _byteVerificationService.DecodeBase64ToBytes(request.Received_data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid payload format.");
                    var errPayload = BuildPacket(false);
                    return Result<string>.Ok(payloadHex);
                }
                var isValid = _byteVerificationService.VerifyByte5(bytes5);
                if (!isValid)
                    return Result<string>.Ok(payloadHex);
                var unique4 = bytes5[..4];
                var spec = new TicketByUniquePartSpecification(unique4);
                var ticket = await _ticketRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
                //var http = _httpContextAccessor.HttpContext;
                //var url = "";

                //if (http != null)
                //{
                //    var forwarded = http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                //    var clientIp = forwarded?.Split(',')[0]?.Trim() ?? http.Connection.RemoteIpAddress?.ToString();

                //    // جِيب البروتوكول الفعلي (http / https)
                //    var scheme = http.Request.Scheme;

                //    // لو البروتوكول فاضي لأي سبب، خليه http كـ fallback
                //    if (string.IsNullOrWhiteSpace(scheme))
                //        scheme = "http";

                //    // بني الـ URL النهائي
                //    url = $"{scheme}://{clientIp}";

                //    _logger.LogInformation("Client Request IP = {ClientIp} | Scheme = {Scheme}", clientIp, scheme);
                //}
                //var httpOk = await _sendProtocolService.SendHttpBinaryAsync(url, payload, cancellationToken);

                //if (!httpOk)
                //    return Result.Fail(Error.Security("Ticket.Exit.Fail", "Ticket exit failed."));

                return Result<string>.Ok(payloadHex);
            }
            var payload2 = BuildPacket(false);
            string payloadHex2 = BitConverter.ToString(payload2).Replace("-", "");

            var http2 = _httpContextAccessor.HttpContext;
            var url2 = "";

            if (http2 != null)
            {
                var forwarded = http2.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                var clientIp = forwarded?.Split(',')[0]?.Trim() ?? http2.Connection.RemoteIpAddress?.ToString();

                // جِيب البروتوكول الفعلي (http / https)
                var scheme = http2.Request.Scheme;

                // لو البروتوكول فاضي لأي سبب، خليه http كـ fallback
                if (string.IsNullOrWhiteSpace(scheme))
                    scheme = "http";

                // بني الـ URL النهائي
                url2 = $"{scheme}://{clientIp}";

                _logger.LogInformation("Client Request IP = {ClientIp} | Scheme = {Scheme}", clientIp, scheme);
            }
            //var httpOk2 = await _sendProtocolService.SendHttpBinaryAsync(url2, payload2, cancellationToken);

            //if (!httpOk2)
            //    return Result.Fail(Error.Security("Ticket.Exit.Fail", "Ticket exit failed."));

            return Result<string>.Ok(payloadHex2);
        }

        private byte[] BuildPacket(bool flag)
        {
            // { Start = 0x7B , End = 0x7D }
            byte start = 0x7B;
            byte end = 0x7D;

            // flag byte
            byte flagByte = flag ? (byte)0x02 : (byte)0x01;

            // reserved byte
            byte reserved = 0x00;

            // build the packet
            byte[] packet = new byte[]
            {
                start,
                flagByte,
                reserved,
                end
            };

            return packet;
        }
    }
}