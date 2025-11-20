using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.Auth.Command.LogOut
{
    public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<ParkingGate> _gateRepository;

        public LogoutCommandHandler(ITokenReader tokenReader, IHttpContextAccessor httpContextAccessor, IGenericRepository<ParkingGate> gateRepository)
        {
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _gateRepository = gateRepository ?? throw new ArgumentNullException(nameof(gateRepository));
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var dto = _httpContextAccessor.HttpContext?.ReadToken(_tokenReader);
            if (dto == null || dto.GateId == null)
            {
                return Result.Fail(new Error("Token not found", "Token not found", ErrorType.NotFound));
            }
            if (dto.GateId != null)
            {
                var gate = await _gateRepository.GetByIdAsync((Guid)dto.GateId, cancellationToken);
                gate?.SetIsOccupied(false, null, DateTime.UtcNow);
                await _gateRepository.SaveChangesAsync(cancellationToken);

                return Result.Ok();
            }
            return Result.Ok();
        }
    }
}