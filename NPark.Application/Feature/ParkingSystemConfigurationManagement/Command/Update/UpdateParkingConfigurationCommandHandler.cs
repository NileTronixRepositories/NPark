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

namespace NPark.Application.Feature.ParkingSystemConfigurationManagement.Command.Update
{
    public class UpdateParkingConfigurationCommandHandler : ICommandHandler<UpdateParkingConfigurationCommand>
    {
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;
        private readonly ILogger<UpdateParkingConfigurationCommandHandler> _logger;
        private readonly IAuditLogger _auditLogger;

        public UpdateParkingConfigurationCommandHandler(IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository,
            IGenericRepository<ParkingGate> parkingGateRepository,
            IAuditLogger auditLogger,
            ILogger<UpdateParkingConfigurationCommandHandler> logger)
        {
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
            _parkingGateRepository = parkingGateRepository ?? throw new ArgumentNullException(nameof(parkingGateRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task<Result> Handle(UpdateParkingConfigurationCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetParkingSystemConfigurationForUpdateSpecification();
            var entity = await _parkingSystemConfigurationRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (entity is null)
            {
                entity = ParkingSystemConfiguration.Create(
                       request.EntryGatesCount,
                       request.ExitGatesCount,
                       request.AllowedParkingSlots,
                       request.PriceType,
                       request.VehiclePassengerData,
                       request.PrintType,
                       request.DateTimeFlag,
                       request.TicketIdFlag,
                       request.FeesFlag,
                       request.PricingSchemaId == Guid.Empty ? null : request.PricingSchemaId,
                       request.gracePeriodMinutes == null ? null : TimeSpan.FromMinutes(request.gracePeriodMinutes.Value)
                   );

                var specf = new GetParkingGateBySystemConfiguration(entity.Id);
                var entities = await _parkingGateRepository.ListWithSpecAsync(specf, cancellationToken);
                if (entities is not null && entities.Count > 0)
                {
                    _parkingGateRepository.DeleteRange(entities);
                    await _parkingGateRepository.SaveChangesAsync(cancellationToken);
                }

                foreach (var gate in request.EntryGates)
                {
                    entity.AddParkingGate(ParkingGate.Create(gate.GateNumber, GateType.Entrance, gate.LprIp, gate.PcIp));
                }
                foreach (var gate in request.ExitGates)
                {
                    entity.AddParkingGate(ParkingGate.Create(gate.GateNumber, GateType.Exit, gate.LprIp, gate.PcIp));
                }

                await _parkingSystemConfigurationRepository.AddAsync(entity, cancellationToken);
                await _parkingSystemConfigurationRepository.SaveChangesAsync(cancellationToken);

                await _auditLogger.LogAsync(
                  new AuditLogEntry(
                      EventName: "CreateParkingSystemConfiguration",
                      EventCategory: "ParkingSystem",
                      IsSuccess: true,
                      StatusCode: 201,  // Created
                      Extra: new
                      {
                          request.EntryGatesCount,
                          request.ExitGatesCount,
                          request.AllowedParkingSlots,
                          request.PriceType
                      }),
                  cancellationToken);
                return Result.Ok();
            }
            else
            {
                entity.Update(
                       entity,
                       request.EntryGatesCount,
                       request.ExitGatesCount,
                       request.AllowedParkingSlots,
                       request.PriceType,
                       request.VehiclePassengerData,
                       request.PrintType,
                       request.DateTimeFlag,
                       request.TicketIdFlag,
                       request.FeesFlag,
                       request.PricingSchemaId == Guid.Empty ? null : request.PricingSchemaId,
                          request.gracePeriodMinutes == null ? null : TimeSpan.FromMinutes(request.gracePeriodMinutes.Value)
                   );
                await _parkingSystemConfigurationRepository.SaveChangesAsync(cancellationToken);

                var specf = new GetParkingGateBySystemConfiguration(entity.Id);
                var entities = await _parkingGateRepository.ListWithSpecAsync(specf, cancellationToken);
                if (entities is not null && entities.Count > 0)
                {
                    _parkingGateRepository.DeleteRange(entities);
                    await _parkingGateRepository.SaveChangesAsync(cancellationToken);
                }

                var list = new List<ParkingGate>();
                foreach (var gate in request.EntryGates)
                {
                    list.Add(ParkingGate.Create(gate.GateNumber, GateType.Entrance, gate.LprIp, gate.PcIp, entity.Id));
                }
                foreach (var gate in request.ExitGates)
                {
                    list.Add(ParkingGate.Create(gate.GateNumber, GateType.Exit, gate.LprIp, gate.PcIp, entity.Id));
                }
                await _parkingGateRepository.AddRangeAsync(list, cancellationToken);
                await _parkingGateRepository.SaveChangesAsync(cancellationToken);

                await _auditLogger.LogAsync(
                   new AuditLogEntry(
                       EventName: "UpdateParkingSystemConfiguration",
                       EventCategory: "ParkingSystem",
                       IsSuccess: true,
                       StatusCode: 200,  // OK
                       Extra: new
                       {
                           request.EntryGatesCount,
                           request.ExitGatesCount,
                           request.AllowedParkingSlots,
                           request.PriceType
                       }),
                   cancellationToken);

                return Result.Ok();
            }
        }
    }
}