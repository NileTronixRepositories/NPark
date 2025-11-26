using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.ParkingSystemConfigurationManagement.Command.Update
{
    public sealed class UpdateParkingConfigurationCommandHandler
    : ICommandHandler<UpdateParkingConfigurationCommand>
    {
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<UpdateParkingConfigurationCommandHandler> _logger;

        public UpdateParkingConfigurationCommandHandler(
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository,
            IGenericRepository<ParkingGate> parkingGateRepository,
            IGenericRepository<Ticket> ticketRepository,
            IAuditLogger auditLogger,
            ILogger<UpdateParkingConfigurationCommandHandler> logger)
        {
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository
                ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
            _parkingGateRepository = parkingGateRepository
                ?? throw new ArgumentNullException(nameof(parkingGateRepository));
            _ticketRepository = ticketRepository
                ?? throw new ArgumentNullException(nameof(ticketRepository));
            _auditLogger = auditLogger
                ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(
            UpdateParkingConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var spec = new GetParkingSystemConfigurationForUpdateSpecification();
                var configuration = await _parkingSystemConfigurationRepository
                    .FirstOrDefaultWithSpecAsync(spec, cancellationToken);

                if (configuration is null)
                {
                    return await CreateConfigurationAsync(request, cancellationToken);
                }

                return await UpdateConfigurationAsync(configuration, request, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("UpdateParkingConfiguration operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating parking configuration.");

                return Result.Fail(
                    new Error(
                        Code: "ParkingConfiguration.Update.Unexpected",
                        Message: ErrorMessage.ParkingConfigurationUpdate_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Create configuration + gates from scratch
        // --------------------------------------------------------
        private async Task<Result> CreateConfigurationAsync(
            UpdateParkingConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            var pricingSchemaId =
                request.PricingSchemaId == Guid.Empty ? null : request.PricingSchemaId;

            var gracePeriod = request.gracePeriodMinutes.HasValue
                ? TimeSpan.FromMinutes(request.gracePeriodMinutes.Value)
                : (TimeSpan?)null;

            var configuration = ParkingSystemConfiguration.Create(
                entryGatesCount: request.EntryGatesCount,
                exitGatesCount: request.ExitGatesCount,
                allowedParkingSlots: request.AllowedParkingSlots,
                priceType: request.PriceType,
                vehiclePassengerData: request.VehiclePassengerData,
                printType: request.PrintType,
                dateTimeFlag: request.DateTimeFlag,
                ticketIdFlag: request.TicketIdFlag,
                feesFlag: request.FeesFlag,
                pricingSchemaId: pricingSchemaId,
                gracePeriod: gracePeriod);

            // Entry gates
            foreach (var gate in request.EntryGates)
            {
                configuration.AddParkingGate(
                    ParkingGate.Create(
                        gateNumber: gate.GateNumber,
                        gateType: GateType.Entrance,
                        lprIp: gate.LprIp,
                        pcIp: gate.PcIp));
            }

            // Exit gates
            foreach (var gate in request.ExitGates)
            {
                configuration.AddParkingGate(
                    ParkingGate.Create(
                        gateNumber: gate.GateNumber,
                        gateType: GateType.Exit,
                        lprIp: gate.LprIp,
                        pcIp: gate.PcIp));
            }

            await _parkingSystemConfigurationRepository.AddAsync(configuration, cancellationToken);
            await _parkingSystemConfigurationRepository.SaveChangesAsync(cancellationToken);

            await SafeAuditAsync(
                eventName: "CreateParkingSystemConfiguration",
                statusCode: 201,
                request: request,
                configurationId: configuration.Id,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Parking system configuration created successfully. ConfigurationId: {ConfigurationId}",
                configuration.Id);

            return Result.Ok();
        }

        // --------------------------------------------------------
        // Update existing configuration + sync gates
        // --------------------------------------------------------
        private async Task<Result> UpdateConfigurationAsync(
            ParkingSystemConfiguration configuration,
            UpdateParkingConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            var pricingSchemaId =
                request.PricingSchemaId == Guid.Empty ? null : request.PricingSchemaId;

            var gracePeriod = request.gracePeriodMinutes.HasValue
                ? TimeSpan.FromMinutes(request.gracePeriodMinutes.Value)
                : (TimeSpan?)null;

            // 1) Update configuration data
            configuration.Update(
                configuration,
                request.EntryGatesCount,
                request.ExitGatesCount,
                request.AllowedParkingSlots,
                request.PriceType,
                request.VehiclePassengerData,
                request.PrintType,
                request.DateTimeFlag,
                request.TicketIdFlag,
                request.FeesFlag,
                pricingSchemaId,
                gracePeriod);

            await _parkingSystemConfigurationRepository.SaveChangesAsync(cancellationToken);

            // 2) Load existing gates for this configuration
            var gateSpec = new GetParkingGateBySystemConfiguration(configuration.Id);
            var existingGates = (await _parkingGateRepository
                .ListWithSpecAsync(gateSpec, cancellationToken)).ToList();

            // 3) Build maps
            var existingGateById = existingGates
                .Where(g => g.Id != Guid.Empty)
                .ToDictionary(g => g.Id, g => g);

            var requestedEntryGates = request.EntryGates;
            var requestedExitGates = request.ExitGates;

            var requestedGateIds = request.EntryGates
                .Concat(request.ExitGates)
                .Where(g => g.GateId.HasValue)
                .Select(g => g.GateId!.Value)
                .ToHashSet();

            // 4) Update existing gates that appear in request
            UpdateExistingGates(existingGateById, requestedEntryGates, GateType.Entrance);
            UpdateExistingGates(existingGateById, requestedExitGates, GateType.Exit);

            // 5) Delete gates that exist in DB but not in request (after checking tickets)
            var gatesToDelete = existingGates
                .Where(g => !requestedGateIds.Contains(g.Id))
                .ToList();

            if (gatesToDelete.Count > 0)
            {
                // Check tickets
                foreach (var gate in gatesToDelete)
                {
                    var hasTickets = await _ticketRepository
                        .IsExistAsync(t => t.GateId == gate.Id, cancellationToken);

                    if (hasTickets)
                    {
                        _logger.LogWarning(
                            "Cannot delete gate with tickets. GateId: {GateId}, GateNumber: {GateNumber}",
                            gate.Id, gate.GateNumber);

                        return Result.Fail(
                            new Error(
                                Code: "Gate.HasTickets",
                                Message: ErrorMessage.Gate_HasTickets,
                                Type: ErrorType.Conflict));
                    }
                }

                _parkingGateRepository.DeleteRange(gatesToDelete);
                await _parkingGateRepository.SaveChangesAsync(cancellationToken);
            }

            // 6) Add new gates (GateId == null in request)
            var newGates = new List<ParkingGate>();

            foreach (var gate in requestedEntryGates.Where(g => !g.GateId.HasValue))
            {
                newGates.Add(
                    ParkingGate.Create(
                        gateNumber: gate.GateNumber,
                        gateType: GateType.Entrance,
                        lprIp: gate.LprIp,
                        pcIp: gate.PcIp,
                        id: configuration.Id));
            }

            foreach (var gate in requestedExitGates.Where(g => !g.GateId.HasValue))
            {
                newGates.Add(
                    ParkingGate.Create(
                        gateNumber: gate.GateNumber,
                        gateType: GateType.Exit,
                        lprIp: gate.LprIp,
                        pcIp: gate.PcIp,
                        id: configuration.Id));
            }

            if (newGates.Count > 0)
            {
                await _parkingGateRepository.AddRangeAsync(newGates, cancellationToken);
                await _parkingGateRepository.SaveChangesAsync(cancellationToken);
            }

            // 7) Audit log
            await SafeAuditAsync(
                eventName: "UpdateParkingSystemConfiguration",
                statusCode: 200,
                request: request,
                configurationId: configuration.Id,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Parking system configuration updated successfully. ConfigurationId: {ConfigurationId}",
                configuration.Id);

            return Result.Ok();
        }

        // --------------------------------------------------------
        // Helper: update existing gates from GateInfo
        // --------------------------------------------------------
        private static void UpdateExistingGates(
            IDictionary<Guid, ParkingGate> existingGateById,
            IEnumerable<GateInfo> gateInfos,
            GateType gateType)
        {
            foreach (var gateInfo in gateInfos.Where(g => g.GateId.HasValue))
            {
                if (!existingGateById.TryGetValue(gateInfo.GateId!.Value, out var gate))
                {
                    continue; // أو ممكن تعتبرها Error حسب البيزنس
                }

                // بافترض إن عندك Domain method لتحديث بيانات الجيت
                // لو مفيش، ممكن تعمل Setters أو Method زي UpdateGateInfo
                gate.UpdateGateInfo(
                    gateNumber: gateInfo.GateNumber,
                    gateType: gateType,
                    lprIp: gateInfo.LprIp,
                    pcIp: gateInfo.PcIp);
            }
        }

        // --------------------------------------------------------
        // Helper: Safe audit logging
        // --------------------------------------------------------
        private async Task SafeAuditAsync(
            string eventName,
            int statusCode,
            UpdateParkingConfigurationCommand request,
            int configurationId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _auditLogger.LogAsync(
                    new AuditLogEntry(
                        EventName: eventName,
                        EventCategory: "ParkingSystem",
                        IsSuccess: true,
                        StatusCode: statusCode,
                        UserId: null,
                        GateId: null,
                        Extra: new
                        {
                            ConfigurationId = configurationId,
                            request.EntryGatesCount,
                            request.ExitGatesCount,
                            request.AllowedParkingSlots,
                            request.PriceType,
                            request.gracePeriodMinutes,
                            TotalEntryGates = request.EntryGates.Count,
                            TotalExitGates = request.ExitGates.Count,
                            LoggedAt = DateTime.UtcNow
                        }),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Audit logging failed for {EventName}. ConfigurationId: {ConfigurationId}",
                    eventName, configurationId);
            }
        }
    }
}