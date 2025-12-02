using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Logging;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.PricingSchemaManagement.Command.Add
{
    public sealed class AddPricingSchemaCommandHandler
      : ICommandHandler<AddPricingSchemaCommand>
    {
        private readonly IGenericRepository<PricingScheme> _repository;
        private readonly ILogger<AddPricingSchemaCommandHandler> _logger;

        public AddPricingSchemaCommandHandler(
            IGenericRepository<PricingScheme> repository,
            ILogger<AddPricingSchemaCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(
            AddPricingSchemaCommand request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 1) Build PricingScheme entity based on request
                var pricingScheme = BuildPricingScheme(request);

                // 2) Persist
                await _repository.AddAsync(pricingScheme, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Pricing scheme '{Name}' added successfully at {DateTimeUtc}.",
                    pricingScheme.Name,
                    DateTime.UtcNow);

                return Result.Ok();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("AddPricingSchema operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while adding pricing scheme. Name: {Name}, DurationType: {DurationType}, IsRepeated: {IsRepeated}.",
                    request.Name,
                    request.DurationType,
                    request.IsRepeated);

                return Result.Fail(
                    new Error(
                        Code: "PricingScheme.Add.Unexpected",
                        Message: ErrorMessage.PricingSchemeAddUnexpected,
                        Type: ErrorType.Infrastructure));
            }
        }

        // ---------------------------------------------
        // Helper: centralize creation logic
        // ---------------------------------------------
        private static PricingScheme BuildPricingScheme(AddPricingSchemaCommand request)
        {
            // لو الخطة مكررة (IsRepeated = true):
            // - نستخدم DurationType.Hours
            // - نشتغل بـ TotalHours فقط
            if (request.IsRepeated)
            {
                return PricingScheme.Create(
                    name: request.Name,
                    durationType: DurationType.Hours,
                    startTime: null,
                    endTime: null,
                    isRepeated: true,
                    salary: request.Price,
                    totalDays: null,
                    totalHours: request.TotalHours);
            }

            // لو مش مكررة: نمشي على نوع المدة
            return request.DurationType switch
            {
                DurationType.Hours => CreateHourlyScheme(request),
                DurationType.Days => CreateDailyScheme(request),
                DurationType.Years => CreateYearlyScheme(request),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(request.DurationType),
                    request.DurationType,
                    "Unsupported duration type for pricing scheme.")
            };
        }

        private static PricingScheme CreateHourlyScheme(AddPricingSchemaCommand request)
        {
            return PricingScheme.Create(
                name: request.Name,
                durationType: DurationType.Hours,
                startTime: request.StartTime,
                endTime: request.EndTime,
                isRepeated: false,
                salary: request.Price,
                totalDays: null,
                totalHours: request.TotalHours);
        }

        private static PricingScheme CreateDailyScheme(AddPricingSchemaCommand request)
        {
            return PricingScheme.Create(
                name: request.Name,
                durationType: DurationType.Days,
                startTime: request.StartTime,
                endTime: request.EndTime,
                isRepeated: false,
                salary: request.Price,
                totalDays: request.TotalDays,
                totalHours: null);
        }

        private static PricingScheme CreateYearlyScheme(AddPricingSchemaCommand request)
        {
            // لو عندك TotalDays للسنين ممكن تستخدمه، هنا fallback = 365
            var totalDays = request.TotalDays ?? 365;

            return PricingScheme.Create(
                name: request.Name,
                durationType: DurationType.Years,
                startTime: request.StartTime,
                endTime: request.EndTime,
                isRepeated: false,
                salary: request.Price,
                totalDays: totalDays,
                totalHours: null);
        }
    }
}