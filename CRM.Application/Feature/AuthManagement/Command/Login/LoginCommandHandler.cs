using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Application.Abstraction.Security;
using CRM.Application.Shared.Dto;
using CRM.Application.Specification.AccountSpecification;
using CRM.Application.Specification.AuthSpecification;
using CRM.Domain.Entities;
using CRM.Domain.Resources;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Feature.AuthManagement.Command.Login
{
    internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, UserTokenDto>
    {
        private readonly IGenericRepository<SuperAdmin> _superAdminRepository;
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;

        public LoginCommandHandler(IGenericRepository<SuperAdmin> superAdminRepository, ILogger<LoginCommandHandler> logger,
            IPasswordService passwordService, IJwtProvider jwtProvider, IGenericRepository<Account> accountRepository)
        {
            _superAdminRepository = superAdminRepository ?? throw new ArgumentNullException(nameof(superAdminRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<Result<UserTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            //1- Get User by Email
            var spec = new LoginSpec(request);
            var user = await _superAdminRepository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (user is null)
            {
                //Check for Account
                var accountSpec = new GetAccountByEmailSpec(request.Email);
                var account = await _accountRepository.FirstOrDefaultWithSpecAsync(accountSpec, cancellationToken);
                if (account is null)
                {
                    return Result<UserTokenDto>.Fail(new Error(
                            Code: "user.NotFound",
                            Message: ErrorMessage.WrongLoginCredintial,
                            Type: ErrorType.Security
                            ));
                }
                {
                    //2- Check Password
                    var accountresult = _passwordService.Verify(request.Password, account.Password);
                    if (!accountresult)
                    {
                        _logger.LogInformation("User {Email} found in database", account.Email);

                        return Result<UserTokenDto>.Fail(new Error(
                            Code: "user.NotFound",
                            Message: ErrorMessage.WrongLoginCredintial,
                            Type: ErrorType.Security
                            ));
                    }
                    var accounttoken = await _jwtProvider.GenerateAccount(account, cancellationToken);
                    return Result<UserTokenDto>.Ok(accounttoken);
                }
            }

            //2- Check Password
            var result = _passwordService.Verify(request.Password, user.Password);
            if (!result)
            {
                _logger.LogInformation("User {Email} found in database", user.Email);

                return Result<UserTokenDto>.Fail(new Error(
                    Code: "user.NotFound",
                    Message: ErrorMessage.WrongLoginCredintial,
                    Type: ErrorType.Security
                    ));
            }

            //3- Generate Token
            var token = await _jwtProvider.Generate(user, cancellationToken);
            _logger.LogInformation("Login by {Email} is  Success", user.Email);
            return Result<UserTokenDto>.Ok(token);
        }
    }
}