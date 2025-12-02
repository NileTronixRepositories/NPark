using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public sealed class SelectGateCommandHandler
       : ICommandHandler<SelectGateCommand, UserTokenDto>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly IGenericRepository<ParkingGate> _gateRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<SelectGateCommandHandler> _logger;

        public SelectGateCommandHandler(
            IHttpContextAccessor contextAccessor,
            ITokenReader tokenReader,
            IGenericRepository<ParkingGate> gateRepository,
            IJwtProvider jwtProvider,
            IAuditLogger auditLogger,
            IGenericRepository<User> userRepository,
            ILogger<SelectGateCommandHandler> logger)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            _gateRepository = gateRepository ?? throw new ArgumentNullException(nameof(gateRepository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserTokenDto>> Handle(
            SelectGateCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Read token info
                // ---------------------------
                var tokenInfo = _contextAccessor.HttpContext?.ReadToken(_tokenReader);

                if (tokenInfo is null)
                {
                    _logger.LogWarning("SelectGate failed: token is null.");
                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Auth.Token.Invalid",
                            Message: ErrorMessage.AuthTokenInvalid,
                            Type: ErrorType.Security));
                }

                if (!tokenInfo.UserId.HasValue)
                {
                    _logger.LogWarning("SelectGate failed: UserId is missing in token.");
                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Auth.Token.UserIdMissing",
                            Message: ErrorMessage.AuthUserNotFound,
                            Type: ErrorType.Security));
                }

                var userId = tokenInfo.UserId.Value;

                // ---------------------------
                // 2) Load user
                // ---------------------------
                var userEntity = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (userEntity is null)
                {
                    _logger.LogWarning("SelectGate failed: user not found. UserId: {UserId}", userId);
                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Auth.User.NotFound",
                            Message: ErrorMessage.AuthUserNotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 3) Load gate
                // ---------------------------
                var gateEntity = await _gateRepository.GetByIdAsync(request.GateId, cancellationToken);
                if (gateEntity is null)
                {
                    _logger.LogWarning("SelectGate failed: gate not found. GateId: {GateId}", request.GateId);
                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Gate.NotFound",
                            Message: ErrorMessage.GateNotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 4) Check role vs gate type
                // ---------------------------
                var role = tokenInfo.Role;
                if (!string.IsNullOrWhiteSpace(role))
                {
                    if (role == "EntranceCashier" && gateEntity.GateType == GateType.Exit)
                    {
                        _logger.LogWarning(
                            "SelectGate failed: EntranceCashier cannot select Exit gate. UserId: {UserId}, GateId: {GateId}",
                            userId, gateEntity.Id);

                        return Result<UserTokenDto>.Fail(
                            new Error(
                                Code: "Gate.RoleMismatch",
                                Message: ErrorMessage.GateRoleMismatch,
                                Type: ErrorType.Security));
                    }

                    if (role == "ExitCashier" && gateEntity.GateType == GateType.Entrance)
                    {
                        _logger.LogWarning(
                            "SelectGate failed: ExitCashier cannot select Entrance gate. UserId: {UserId}, GateId: {GateId}",
                            userId, gateEntity.Id);

                        return Result<UserTokenDto>.Fail(
                            new Error(
                                Code: "Gate.RoleMismatch",
                                Message: ErrorMessage.GateRoleMismatch,
                                Type: ErrorType.Security));
                    }
                }

                // ---------------------------
                // 5) Check if gate is occupied
                // ---------------------------
                if (gateEntity.IsOccupied == true &&
                    gateEntity.OccupiedBy.HasValue &&
                    gateEntity.OccupiedBy.Value != userId)
                {
                    _logger.LogWarning(
                        "SelectGate failed: gate is occupied by another user. GateId: {GateId}, OccupiedBy: {OccupiedBy}",
                        gateEntity.Id, gateEntity.OccupiedBy);

                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Gate.Occupied",
                            Message: ErrorMessage.GateOccupied,
                            Type: ErrorType.Conflict));
                }

                // ---------------------------
                // 6) Mark gate as occupied by this user
                // ---------------------------
                gateEntity.SetIsOccupied(
                    isOccupied: true,
                    occupiedBy: userId,
                    occupiedTime: DateTime.UtcNow);

                await _gateRepository.SaveChangesAsync(cancellationToken);

                // ---------------------------
                // 7) Generate token (user + gate)
                // ---------------------------
                var token = await _jwtProvider.Generate(userEntity, gateEntity);

                // Attach gate peripheral info
                token.GateDevicePeripheral = new GateDevicePeripheral
                {
                    LprIp = gateEntity.LprIp,
                    PcIp = gateEntity.PcIp,
                    HasLpr = gateEntity.HasLpr,
                    HasPc = gateEntity.HasPc,
                    GateNumber = gateEntity.GateNumber,
                    GateType = gateEntity.GateType
                };

                // ---------------------------
                // 8) Audit log (Safe)
                // ---------------------------
                await SafeAuditGateSelectedAsync(userId, gateEntity, cancellationToken);

                _logger.LogInformation(
                    "Gate selected successfully. UserId: {UserId}, GateId: {GateId}",
                    userId, gateEntity.Id);

                return Result<UserTokenDto>.Ok(token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SelectGate operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while selecting gate.");
                return Result<UserTokenDto>.Fail(
                    new Error(
                        Code: "Gate.Select.Unexpected",
                        Message: ErrorMessage.GateSelectUnexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Safe audit logging
        // --------------------------------------------------------
        private async Task SafeAuditGateSelectedAsync(
            Guid userId,
            ParkingGate gate,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "GateSelected",
                        EventCategory: "Gate",
                        IsSuccess: true,
                        StatusCode: StatusCodes.Status200OK,
                        UserId: userId,
                        GateId: gate.Id,
                        Extra: new
                        {
                            gate.GateNumber,
                            GateType = gate.GateType.ToString(),
                            SelectedAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for GateSelected. UserId: {UserId}, GateId: {GateId}",
                    userId, gate.Id);
            }
        }
    }
}