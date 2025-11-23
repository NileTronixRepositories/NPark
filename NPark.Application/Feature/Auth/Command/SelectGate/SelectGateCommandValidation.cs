using BuildingBlock.Application.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using NPark.Application.Abstraction.Security;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Domain.Entities;
using NPark.Domain.Enums;

namespace NPark.Application.Feature.Auth.Command.SelectGate
{
    public class SelectGateCommandValidation : AbstractValidator<SelectGateCommand>
    {
        private readonly IGenericRepository<ParkingGate> _gateInfoRepository;
        private readonly ITokenReader _tokenReader;
        private readonly IHttpContextAccessor _contextAccessor;

        public SelectGateCommandValidation(IGenericRepository<ParkingGate> gateInfoRepository,
            ITokenReader tokenReader,
            IHttpContextAccessor contextAccessor
       )
        {
            _gateInfoRepository = gateInfoRepository;
            _tokenReader = tokenReader;
            _contextAccessor = contextAccessor;

            // التحقق من UserId في الـ Token
            RuleFor(x => x)
                .MustAsync(async (command, cancellationToken) =>
                {
                    var tokenInfo = await GetTokenInfoAsync(_contextAccessor, _tokenReader);
                    return tokenInfo?.UserId.HasValue == true;
                })
                .WithMessage("User ID is missing in the token. / معرف المستخدم مفقود في التوكن.");

            RuleFor(x => x.GateNumber)
                .NotEmpty()
                .WithMessage("Gate number is required. / رقم البوابة مطلوب.")
                .MustAsync(async (gateNumber, cancellationToken) =>
                    await IsValidGateAsync(int.Parse(gateNumber.Split(' ')[0]), cancellationToken))
                .WithMessage("Invalid Gate Number. / رقم البوابة غير صالح.");

            // التحقق من تطابق Role مع نوع البوابة
            RuleFor(x => x)
                .MustAsync(async (command, cancellationToken) =>
                {
                    var tokenInfo = await GetTokenInfoAsync(_contextAccessor, _tokenReader);
                    var gateEntity = await GetGateEntityAsync(int.Parse(command.GateNumber.Split(' ')[0]), command.GateType, cancellationToken);

                    if (tokenInfo?.Role == "EntranceCashier" && gateEntity?.GateType == GateType.Exit)
                        return false;

                    if (tokenInfo?.Role == "ExitCashier" && gateEntity?.GateType == GateType.Entrance)
                        return false;

                    return true;
                })
                .WithMessage("Invalid Gate Type for your role. / نوع البوابة غير صحيح لدورك.");

            RuleFor(x => x)
                .MustAsync(async (command, cancellationToken) =>
                {
                    var gateEntity = await GetGateEntityAsync(int.Parse(command.GateNumber.Split(' ')[0]), command.GateType, cancellationToken);
                    return gateEntity?.IsOccupied != true;
                })
                .WithMessage("Gate is Occupied. / البوابة مشغولة.");
        }

        private async Task<ParkingGate?> GetGateEntityAsync(int gateNumber, GateType gateType, CancellationToken cancellationToken)
        {
            return await _gateInfoRepository.FirstOrDefaultWithSpecAsync(new GetGateByGateNumberSpec(gateNumber, gateType), cancellationToken);
        }

        private async Task<TokenInfoDto?> GetTokenInfoAsync(IHttpContextAccessor accessor, ITokenReader tokenReader)
        {
            var tokenInfo = _contextAccessor.HttpContext?.ReadToken(_tokenReader);
            return tokenInfo;
        }

        private async Task<bool> IsValidGateAsync(int gateNumber, CancellationToken cancellationToken)
        {
            var gateEntity = await _gateInfoRepository.IsExistAsync(g => g.GateNumber == gateNumber, cancellationToken);
            return gateEntity;
        }
    }
}