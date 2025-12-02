using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;

namespace NPark.Application.Feature.Auth.Command.SupervisorLogin
{
    public sealed class SupervisorLoginCommandHandler
         : ICommandHandler<SupervisorLoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ParkingGate> _gateRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<SupervisorLoginCommandHandler> _logger;

        public SupervisorLoginCommandHandler(
            IGenericRepository<User> userRepository,
            IJwtProvider jwtProvider,
            IPasswordService passwordService,
            IAuditLogger auditLogger,
            IGenericRepository<ParkingGate> gateRepository,
            ILogger<SupervisorLoginCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _gateRepository = gateRepository ?? throw new ArgumentNullException(nameof(gateRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UserTokenDto>> Handle(
            SupervisorLoginCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Get user by username
                // ---------------------------
                var spec = new GetUserByUserName(request.UserName);
                var userEntity = await _userRepository
                    .FirstOrDefaultWithSpecAsync(spec, cancellationToken);

                var credentialsValid =
                    userEntity is not null &&
                    _passwordService.Verify(request.Password, userEntity.PasswordHash);

                if (!credentialsValid)
                {
                    _logger.LogWarning(
                        "Supervisor login failed due to invalid credentials. UserName: {UserName}",
                        request.UserName);

                    await SafeAuditLoginFailedAsync(userEntity?.Id, request.UserName, cancellationToken);

                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Auth.InvalidCredentials",
                            Message: "اسم المستخدم أو كلمة المرور غير صحيحة (Invalid username or password).",
                            Type: ErrorType.Security));
                }

                // ---------------------------
                // 2) Load gate by GateNumber + GateType
                // ---------------------------
                var gateSpec = new GetGateByGateNumberSpec(request.GateNumber, request.GateType);
                var gateEntity = await _gateRepository
                    .FirstOrDefaultWithSpecAsync(gateSpec, cancellationToken);

                if (gateEntity is null)
                {
                    _logger.LogWarning(
                        "Supervisor login failed: gate not found. GateNumber: {GateNumber}, GateType: {GateType}",
                        request.GateNumber, request.GateType);

                    return Result<UserTokenDto>.Fail(
                        new Error(
                            Code: "Gate.NotFound",
                            Message: "لم يتم العثور على البوابة المحددة (Gate not found).",
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 3) Generate token for supervisor + gate
                // ---------------------------
                var token = await _jwtProvider.Generate(userEntity!, gateEntity);

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
                // 4) Audit success (Safe)
                // ---------------------------
                await SafeAuditGateSelectedAsync(userEntity!.Id, gateEntity, cancellationToken);

                _logger.LogInformation(
                    "Supervisor login succeeded. UserId: {UserId}, GateId: {GateId}, GateNumber: {GateNumber}",
                    userEntity.Id, gateEntity.Id, gateEntity.GateNumber);

                return Result<UserTokenDto>.Ok(token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "SupervisorLogin operation was canceled. UserName: {UserName}",
                    request.UserName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error during SupervisorLogin. UserName: {UserName}",
                    request.UserName);

                return Result<UserTokenDto>.Fail(
                    new Error(
                        Code: "Auth.SupervisorLogin.Unexpected",
                        Message: "حدث خطأ غير متوقع أثناء تسجيل دخول المشرف، برجاء المحاولة لاحقًا (Unexpected error occurred during supervisor login, please try again later).",
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Safe audit: login failed
        // --------------------------------------------------------
        private async Task SafeAuditLoginFailedAsync(
            Guid? userId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: "SupervisorLoginFailed",
                        EventCategory: "Auth",
                        IsSuccess: false,
                        StatusCode: StatusCodes.Status401Unauthorized,
                        ErrorCode: "Auth.InvalidCredentials",
                        ErrorMessage: "اسم المستخدم أو كلمة المرور غير صحيحة (Invalid username or password).",
                        UserId: userId,
                        Extra: new
                        {
                            UserName = userName,
                            AttemptAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for SupervisorLoginFailed. UserName: {UserName}",
                    userName);
            }
        }

        // --------------------------------------------------------
        // Safe audit: supervisor gate selected
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
                        EventName: "SupervisorGateSelected",
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
                    "Audit logging failed for SupervisorGateSelected. UserId: {UserId}, GateId: {GateId}",
                    userId, gate.Id);
            }
        }
    }
}