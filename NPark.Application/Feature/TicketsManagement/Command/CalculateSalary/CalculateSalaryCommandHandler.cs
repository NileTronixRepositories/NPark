using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
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

        public CalculateSalaryCommandHandler
            (
            IGenericRepository<Ticket> ticketRepository,
            IGenericRepository<OrderPricingSchema> orderPricingSchemaRepository,
            IByteVerificationService byteVerificationService,
            IGenericRepository<ParkingSystemConfiguration> parkingSystemConfigurationRepository
            )
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _byteVerificationService = byteVerificationService ?? throw new ArgumentNullException(nameof(byteVerificationService));
            _parkingSystemConfigurationRepository = parkingSystemConfigurationRepository ?? throw new ArgumentNullException(nameof(parkingSystemConfigurationRepository));
            _orderPricingSchemaRepository = orderPricingSchemaRepository ?? throw new ArgumentNullException(nameof(orderPricingSchemaRepository));
        }

        public async Task<Result<CalculateSalaryCommandResponse>> Handle(CalculateSalaryCommand request, CancellationToken cancellationToken)
        {
            int count = 0;

            var bytes = _byteVerificationService.DecodeBase64ToBytes(request.QrCode);
            var isValidQrCode = _byteVerificationService.VerifyByte5(bytes);
            if (!isValidQrCode)
            {
                return Result<CalculateSalaryCommandResponse>.Fail
                        (new Error("QrCode is not valid", "QrCode is not valid", ErrorType.Validation));
            }
            var spec = new TicketByUniquePartSpecification(bytes[..4]);
            var ticketEnttiy = await _ticketRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (ticketEnttiy is null)
            {
                return Result<CalculateSalaryCommandResponse>.Fail
                        (new Error("Ticket not found", "Ticket not found", ErrorType.NotFound));
            }
            TimeSpan enterDate = DateTime.Now - ticketEnttiy.StartDate;
            var Configspec = new GetParkingSystemConfigurationForUpdateSpecification();
            var configuration = await _parkingSystemConfigurationRepository
                .FirstOrDefaultWithSpecAsync(Configspec, cancellationToken);
            if (configuration is null)
            {
                return Result<CalculateSalaryCommandResponse>.Fail(new
                    Error("Configuration not found", "Configuration not found", ErrorType.NotFound));
            }
            if (configuration.PriceType == PriceType.Enter)
            {
                var response = new CalculateSalaryCommandResponse
                {
                    TotalSalary = ticketEnttiy.Price,
                    EnterDate = ticketEnttiy.StartDate,
                    IsCollectByCashier = ticketEnttiy.IsCashierCollected,
                    IsExitValid = true
                };
                ticketEnttiy.SetExitDate();
                await _ticketRepository.SaveChangesAsync(cancellationToken);
                return Result<CalculateSalaryCommandResponse>.Ok(response);
            }
            else
            {
                decimal totalSalary = 0;
                var specOrder = new OrderPriceSchemaSpec();
                var orderPricingSchema = await _orderPricingSchemaRepository.ListWithSpecAsync(specOrder, cancellationToken);
                foreach (var order in orderPricingSchema)
                {
                    if (enterDate > TimeSpan.FromHours(order.PricingScheme.TotalHours.Value))
                    {
                        enterDate -= TimeSpan.FromHours(order.PricingScheme.TotalHours.Value);
                        totalSalary += order.PricingScheme.Salary;
                    }
                }

                var response = new CalculateSalaryCommandResponse
                {
                    TotalSalary = totalSalary,
                    EnterDate = ticketEnttiy.StartDate,
                    IsCollectByCashier = ticketEnttiy.IsCashierCollected,
                    IsExitValid = true
                };

                ticketEnttiy.SetExitDate();
                await _ticketRepository.SaveChangesAsync(cancellationToken);

                return Result<CalculateSalaryCommandResponse>.Ok(response);
            }
        }
    }
}