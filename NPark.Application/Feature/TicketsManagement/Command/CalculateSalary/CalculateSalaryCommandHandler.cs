using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.OrderPriceSchemaSpecification;
using NPark.Application.Specifications.ParkingSystemConfigurationSpec;
using NPark.Application.Specifications.TicketSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public sealed class CalculateSalaryCommandHandler : ICommandHandler<CalculateSalaryCommand, CalculateSalaryCommandResponse>
    {
        private readonly IByteVerificationService _byteVerificationService;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository; // <ParkingSystemConfiguration>
        private readonly IGenericRepository<OrderPricingSchema> _orderPricingSchemaRepository;
        private readonly ILogger<CalculateSalaryCommandHandler> _logger;

        public CalculateSalaryCommandHandler
            (
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<OrderPricingSchema> orderPricingSchemaRepository,
            IByteVerificationService byteVerificationService
            , ILogger<CalculateSalaryCommandHandler> logger,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository
            )
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
            _orderPricingSchemaRepository = orderPricingSchemaRepository ?? throw new ArgumentNullException(nameof(orderPricingSchemaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<CalculateSalaryCommandResponse>> Handle(CalculateSalaryCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Validate QR Code
            var bytes = _byteVerificationService.DecodeBase64ToBytes(request.QrCode);
            if (!_byteVerificationService.VerifyByte5(bytes))
            {
                _logger.LogWarning("Invalid QR Code: {QrCode}", request.QrCode);
                return Result<CalculateSalaryCommandResponse>.Fail(new Error("QrCode is not valid", "QrCode is not valid", ErrorType.Validation));
            }

            // Step 2: Retrieve Ticket Entity
            var ticketEntity = await GetTicketEntity(bytes, cancellationToken);
            if (ticketEntity == null)
            {
                return Result<CalculateSalaryCommandResponse>.Fail(new Error("Ticket not found", "Ticket not found", ErrorType.NotFound));
            }

            // Step 3: Get Parking System Configuration
            var configuration = await GetParkingSystemConfiguration(cancellationToken);
            if (configuration == null)
            {
                return Result<CalculateSalaryCommandResponse>.Fail(new Error("Configuration not found", "Configuration not found", ErrorType.NotFound));
            }

            // Step 4: Calculate Salary
            var enterDate = DateTime.Now - ticketEntity.StartDate;
            decimal totalSalary = configuration.PriceType == PriceType.Enter
                                  ? ticketEntity.Price
                                  : CalculateTotalSalary(enterDate, cancellationToken);

            // Step 5: Save Exit Date
            ticketEntity.SetExitDate();
            await _ticketRepository.SaveChangesAsync(cancellationToken);

            // Step 6: Return Response
            var response = new CalculateSalaryCommandResponse
            {
                TotalSalary = totalSalary,
                EnterDate = ticketEntity.StartDate,
                IsCollectByCashier = ticketEntity.IsCashierCollected,
                IsExitValid = true
            };

            return Result<CalculateSalaryCommandResponse>.Ok(response);
        }

        private async Task<Ticket?> GetTicketEntity(byte[] bytes, CancellationToken cancellationToken)
        {
            var spec = new TicketByUniquePartSpecification(bytes[..4]);
            return await _ticketRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
        }

        private async Task<ParkingSystemConfiguration?> GetParkingSystemConfiguration(CancellationToken cancellationToken)
        {
            var spec = new GetParkingSystemConfigurationForUpdateSpecification();
            return await _parkingSystemConfigurationRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
        }

        private decimal CalculateTotalSalary(TimeSpan enterDate, CancellationToken cancellationToken)
        {
            decimal totalSalary = 0;
            var orderPricingSchemas = _orderPricingSchemaRepository.ListWithSpecAsync(new OrderPriceSchemaSpec(), cancellationToken).Result;

            foreach (var order in orderPricingSchemas)
            {
                if (enterDate > TimeSpan.FromHours(order!.PricingScheme.TotalHours!.Value))
                {
                    enterDate -= TimeSpan.FromHours(order.PricingScheme.TotalHours.Value);
                    totalSalary += order.PricingScheme.Salary;
                }
            }

            return totalSalary;
        }
    }
}