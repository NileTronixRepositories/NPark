using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.Auth.Command.LogOut
{
    public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<ParkingGate> _gateRepository;
        private readonly IAuditLogger _auditLogger;

        public LogoutCommandHandler(ITokenReader tokenReader, IHttpContextAccessor httpContextAccessor,
            IGenericRepository<ParkingGate> gateRepository, IAuditLogger auditLogger)
        {
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _gateRepository = gateRepository ?? throw new ArgumentNullException(nameof(gateRepository));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var dto = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (dto == null || !dto.GateId.HasValue)
            {
                return Result.Fail(new Error("Token not found", "Token not found", ErrorType.NotFound));
            }

            var gate = await _gateRepository.GetByIdAsync((Guid)dto.GateId, cancellationToken);

            if (gate is null)
                return Result.Fail(new Error("Gate not found", "Gate not found", ErrorType.NotFound));

            gate?.SetIsOccupied(false, null, DateTime.UtcNow);
            await _gateRepository.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogAsync(
              new AuditLogEntry(
                  EventName: "LogoutSucceeded",
                  EventCategory: "Auth",
                  IsSuccess: true,
                  StatusCode: 200,  // OK
                  UserId: dto.UserId,
                  GateId: gate?.Id,
                  Extra: new { gate?.GateNumber, gate?.GateType }),  // تسجيل بيانات إضافية إذا لزم الأمر
              cancellationToken);

            return Result.Ok();
        }
    }
}