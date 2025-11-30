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
using NPark.Domain.Resource;

namespace NPark.Application.Feature.TicketsManagement.Command.CalculateSalary
{
    public sealed class CalculateSalaryCommandHandler
       : ICommandHandler<CalculateSalaryCommand, CalculateSalaryCommandResponse>
    {
        private readonly IByteVerificationService _byteVerificationService;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<ParkingSystemConfiguration> _parkingSystemConfigurationRepository;
        private readonly IGenericRepository<OrderPricingSchema> _orderPricingSchemaRepository;
        private readonly ILogger<CalculateSalaryCommandHandler> _logger;

        public CalculateSalaryCommandHandler(
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<OrderPricingSchema> orderPricingSchemaRepository,
            IByteVerificationService byteVerificationService,
            ILogger<CalculateSalaryCommandHandler> logger,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _orderPricingSchemaRepository = orderPricingSchemaRepository ?? throw new ArgumentNullException(nameof(orderPricingSchemaRepository));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<CalculateSalaryCommandResponse>> Handle(
            CalculateSalaryCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // ---------------------------
                // 1) Decode & validate QR code
                // ---------------------------
                byte[] bytes;
                try
                {
                    bytes = _byteVerificationService.DecodeBase64ToBytes(request.QrCode);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid QR code format: {QrCode}", request.QrCode);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "QrCode.InvalidFormat",
                            Message: ErrorMessage.QrInvalid,
                            Type: ErrorType.Validation));
                }

                if (bytes is null || bytes.Length < 5)
                {
                    _logger.LogWarning("QR code bytes length is invalid. Length: {Length}", bytes?.Length);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "QrCode.InvalidBytes",
                            Message: ErrorMessage.QrInvalid,
                            Type: ErrorType.Validation));
                }

                if (!_byteVerificationService.VerifyByte5(bytes))
                {
                    _logger.LogWarning("QR code checksum (byte5) verification failed for QR: {QrCode}", request.QrCode);

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "QrCode.InvalidChecksum",
                            Message: ErrorMessage.QrInvalid,
                            Type: ErrorType.Validation));
                }

                // ---------------------------
                // 2) Retrieve Ticket
                // Unique part = first 4 bytes of QR
                // ---------------------------
                var ticketEntity = await GetTicketEntity(bytes, cancellationToken);
                if (ticketEntity is null)
                {
                    _logger.LogWarning("Ticket not found for QR bytes.");

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "Ticket.NotFound",
                            Message: ErrorMessage.Ticket_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 3) Get Parking System Configuration
                // ---------------------------
                var configuration = await GetParkingSystemConfiguration(cancellationToken);
                if (configuration is null)
                {
                    _logger.LogWarning("Parking system configuration not found while calculating salary.");

                    return Result<CalculateSalaryCommandResponse>.Fail(
                        new Error(
                            Code: "Configuration.NotFound",
                            Message: ErrorMessage.Configuration_NotFound,
                            Type: ErrorType.NotFound));
                }

                // ---------------------------
                // 4) Calculate Total Salary
                // ---------------------------
                var duration = DateTime.Now - ticketEntity.StartDate;
                decimal totalSalary;

                if (configuration.PriceType == PriceType.Enter)
                {
                    totalSalary = ticketEntity.Price;
                }
                else
                {
                    totalSalary = await CalculateTotalSalaryAsync(duration, cancellationToken);
                }

                // ---------------------------
                // 5) Set New Price
                // ---------------------------
                ticketEntity.SetPrice(totalSalary);
                await _ticketRepository.SaveChangesAsync(cancellationToken);

                // ---------------------------
                // 6) Build Response
                // ---------------------------
                var response = new CalculateSalaryCommandResponse
                {
                    TotalSalary = totalSalary,
                    EnterDate = ticketEntity.StartDate,
                    IsCollectByCashier = ticketEntity.IsCashierCollected,
                    IsExitValid = true,
                    TicketId = ticketEntity.Id
                };

                return Result<CalculateSalaryCommandResponse>.Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("CalculateSalary operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calculating salary.");
                return Result<CalculateSalaryCommandResponse>.Fail(
                    new Error(
                        Code: "Salary.Calculate.Unexpected",
                        Message: ErrorMessage.SalaryCalculate_Unexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // --------------------------------------------------------
        // Helper: Get ticket by QR unique part
        // --------------------------------------------------------
        private async Task<Ticket?> GetTicketEntity(byte[] qrBytes, CancellationToken cancellationToken)
        {
            // أول 4 Bytes هي الـ UniqueGuidPart المستخدمة في الـ Ticket
            var uniquePart = qrBytes[..4];
            var spec = new TicketByUniquePartSpecification(uniquePart);
            return await _ticketRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
        }

        // --------------------------------------------------------
        // Helper: Get configuration
        // --------------------------------------------------------
        private async Task<ParkingSystemConfiguration?> GetParkingSystemConfiguration(CancellationToken cancellationToken)
        {
            var spec = new GetParkingSystemConfigurationForUpdateSpecification();
            return await _parkingSystemConfigurationRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
        }

        // --------------------------------------------------------
        // Helper: Calculate total salary for duration (EXIT pricing)
        // --------------------------------------------------------
        private async Task<decimal> CalculateTotalSalaryAsync(
            TimeSpan duration,
            CancellationToken cancellationToken)
        {
            decimal totalSalary = 0m;

            var orderPricingSchemas = await _orderPricingSchemaRepository
                .ListWithSpecAsync(new OrderPriceSchemaSpec(), cancellationToken);

            if (orderPricingSchemas is null || !orderPricingSchemas.Any())
            {
                _logger.LogWarning("No order pricing schemas found while calculating salary.");
                return 0m;
            }

            var orderedSchemas = orderPricingSchemas
                .Where(o => o?.PricingScheme?.TotalHours.HasValue == true)
                .OrderBy(o => o.Count)
                .ToList();

            var remaining = duration;
            foreach (var order in orderedSchemas)
            {
                if (remaining <= TimeSpan.Zero)
                {
                    break;
                }

                var hours = order!.PricingScheme.TotalHours!.Value;
                var salary = order.PricingScheme.Salary;

                var slot = TimeSpan.FromHours(hours);

                remaining -= slot;
                totalSalary += salary;
            }

            return totalSalary;
        }
    }
}