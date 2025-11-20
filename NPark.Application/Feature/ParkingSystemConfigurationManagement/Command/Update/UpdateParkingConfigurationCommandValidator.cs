using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;
using NPark.Domain.Enums;
using NPark.Domain.Resource;
using System.Net;

namespace NPark.Application.Feature.ParkingSystemConfigurationManagement.Command.Update
{
    public class UpdateParkingConfigurationCommandValidator : AbstractValidator<UpdateParkingConfigurationCommand>
    {
        private readonly IGenericRepository<PricingScheme> _pricingSchemeRepository;

        public UpdateParkingConfigurationCommandValidator(IGenericRepository<PricingScheme> pricingSchemeRepository)
        {
            _pricingSchemeRepository = pricingSchemeRepository ?? throw new ArgumentNullException(nameof(pricingSchemeRepository));
            RuleFor(x => x.EntryGatesCount)
    .GreaterThanOrEqualTo(1)
    .WithMessage("Entry gates count must be at least 1. \nعدد بوابات الدخول يجب ألا يقل عن 1.");

            RuleFor(x => x.ExitGatesCount)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.Invalid_ExitGatesCount);

            RuleFor(x => x.PriceType)
                .IsInEnum()
                .WithMessage(ErrorMessage.Invalid_PriceType);

            RuleFor(x => x.VehiclePassengerData)
                .IsInEnum()
                .WithMessage(ErrorMessage.Invalid_VehiclePassengerData);

            RuleFor(x => x.PrintType)
                .IsInEnum()
                .WithMessage(ErrorMessage.Invalid_PrintType);

            // ===== Nullable / Optional Fields =====

            RuleFor(x => x.AllowedParkingSlots)
                .GreaterThan(0)
                .When(x => x.AllowedParkingSlots.HasValue)
                .WithMessage(ErrorMessage.Invalid_AllowedParkingSlots);

            // ===== Conditional Restriction =====
            // When PriceType = Enter → PricingSchemaId must be provided

            When(x => x.PriceType == PriceType.Enter, () =>
            {
                RuleFor(x => x.PricingSchemaId)
                    .NotNull()
                    .NotEqual(Guid.Empty)
                    .WithMessage(ErrorMessage.PricingSchemaId_RequiredWhenEnter)
                    .MustAsync(async (id, token) => await _pricingSchemeRepository.IsExistAsync(x => x.Id == id, token));
            });
            // ===== Validation for EntryGatesCount and EntryGates =====
            RuleFor(x => x.EntryGatesCount)
                .Equal(x => x.EntryGates.Count)
                .WithMessage("عدد بوابات الدخول يجب أن يطابق EntryGatesCount / EntryGatesCount must match the number of EntryGates.");

            // ===== Validation for ExitGatesCount and ExitGates =====
            RuleFor(x => x.ExitGatesCount)
                .Equal(x => x.ExitGates.Count)
                .WithMessage("عدد بوابات الخروج يجب أن يطابق ExitGatesCount / ExitGatesCount must match the number of ExitGates.");

            RuleForEach(x => x.EntryGates)
                    .ChildRules(g =>
                    {
                        g.RuleFor(x => x.LprIp)
                        .Must(IsValidIp).WithMessage("EntryGate LPR IP must be a valid IPv4 or IPv6 address.")
                        .When(x => !string.IsNullOrWhiteSpace(x.LprIp));
                        g.RuleFor(x => x.PcIp)
                    .Must(IsValidIp).WithMessage("ExitGate PC IP must be a valid IPv4 or IPv6 address.")
                    .When(x => !string.IsNullOrWhiteSpace(x.PcIp));
                    });

            RuleForEach(x => x.ExitGates)
                .ChildRules(g =>
                {
                    g.RuleFor(x => x.LprIp)
                        .Must(IsValidIp).WithMessage("ExitGate LPR IP must be a valid IPv4 or IPv6 address.")
                        .When(x => !string.IsNullOrWhiteSpace(x.LprIp));
                    g.RuleFor(x => x.PcIp)
                        .Must(IsValidIp).WithMessage("ExitGate PC IP must be a valid IPv4 or IPv6 address.")
                        .When(x => !string.IsNullOrWhiteSpace(x.PcIp));
                });

            RuleFor(x => x)
                .Must(HaveUniqueIps)
                .WithMessage("Each LPR IP must be unique across both EntryGates and ExitGates.");
        }

        private static bool HaveUniqueIps(UpdateParkingConfigurationCommand command)
        {
            var allIps = command.EntryGates
                .Select(g => g.LprIp?.Trim())
                .Concat(command.ExitGates.Select(g => g.LprIp?.Trim()))
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .ToList();

            return allIps.Distinct(StringComparer.OrdinalIgnoreCase).Count() == allIps.Count;
        }

        private static bool IsValidIp(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            return IPAddress.TryParse(ip, out _);
        }
    }
}