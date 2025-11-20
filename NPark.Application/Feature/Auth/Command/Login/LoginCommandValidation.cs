using BuildingBlock.Application.Repositories;
using FluentValidation;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.Login
{
    public sealed class LoginCommandValidation : AbstractValidator<LoginCommand>
    {
        private readonly IGenericRepository<User> _user;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;

        public LoginCommandValidation(IGenericRepository<User> user, IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _parkingGateRepository = parkingGateRepository ?? throw new ArgumentNullException(nameof(parkingGateRepository));
            RuleFor(x => x.UserName).NotEmpty().WithMessage(ErrorMessage.IsRequired)
    .MustAsync(async (username, cancellation) =>
    await _user.IsExistAsync(x => x.Username == username)).WithMessage(ErrorMessage.NotFound);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessage.PasswordRequired);

            //RuleFor(x => x.GateNumber)
            //    .NotEmpty().WithMessage("Gate Number is required");

            //RuleFor(x => x.GateType)
            //    .NotEmpty().WithMessage("Gate Type is required")
            //    .IsInEnum().WithMessage("Invalid Gate Type");
            //RuleFor(x => x)
            //    .MustAsync(async (command, cancellation) =>
            //    {
            //        return await _parkingGateRepository.IsExistAsync(x => x.GateNumber == command.GateNumber && x.GateType == command.GateType);
            //    })
            //    .WithMessage("Gate Not Found")
            //    .MustAsync(async (command, cancellation) =>
            //    {
            //        return await _parkingGateRepository.IsExistAsync(x => x.GateNumber == command.GateNumber && x.GateType == command.GateType && x.IsOccupied != false);
            //    })
            //    .WithMessage("Gate Already Occupied");
        }
    }
}