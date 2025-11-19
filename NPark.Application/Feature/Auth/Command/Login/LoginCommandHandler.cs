using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using NPark.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.ParkingGateSpec;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Domain.Resource;

namespace NPark.Application.Feature.Auth.Command.Login
{
    internal class LoginCommandHandler : ICommandHandler<LoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly IGenericRepository<ParkingGate> _parkingGateRepository;

        public LoginCommandHandler(IGenericRepository<User> repository,
            IJwtProvider jwtProvider, IPasswordService passwordService,
            IGenericRepository<ParkingGate> parkingGateRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _parkingGateRepository = parkingGateRepository ?? throw new ArgumentNullException(nameof(parkingGateRepository));
        }

        public async Task<Result<UserTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetUserByUserName(request.UserName);
            var userEntity = await _repository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            var isValid = _passwordService.Verify(request.Password, userEntity!.PasswordHash);

            if (!isValid)
                return Result<UserTokenDto>.Fail(new Error
                  (ErrorMessage.WrongPassword, ErrorMessage.WrongPassword, ErrorType.Security));

            var specGate = new GetParkingGateByGateNumberAndGateTypeSpecification(request.GateNumber, request.GateType);

            var gateEntity = await _parkingGateRepository.FirstOrDefaultWithSpecAsync(specGate, cancellationToken);
            gateEntity!.SetIsOccupied(true, userEntity.Id, DateTime.UtcNow);
            await _parkingGateRepository.SaveChangesAsync();
            var response = await _jwtProvider.Generate(userEntity);
            return Result<UserTokenDto>.Ok(response);
        }
    }
}