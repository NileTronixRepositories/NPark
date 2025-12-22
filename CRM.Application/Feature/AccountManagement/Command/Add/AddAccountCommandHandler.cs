using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Results;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Feature.AccountManagement.Command.Add
{
    internal sealed class AddAccountCommandHandler : ICommandHandler<AddAccountCommand>
    {
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly ILogger<AddAccountCommandHandler> _logger;
        private readonly IPasswordService _passwordService;

        public AddAccountCommandHandler(IGenericRepository<Account> accountRepository, ILogger<AddAccountCommandHandler> logger,
            IPasswordService passwordService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        public async Task<Result> Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {
            //1- Check for Email Uniqueness
            var existingAccount = await _accountRepository.IsExistAsync(x => x.Email == request.Email, cancellationToken);
            if (existingAccount)
            {
                _logger.LogError("Account with email {0} already exists", request.Email);
                return Result.Fail(new Error(
                    Code: "Account.EmailExists",
                    Message: $"Account with email {request.Email} already exists",
                    Type: ErrorType.Conflict
                    ));
            }

            //2-Check for NameEn Uniqueness
            existingAccount = await _accountRepository.IsExistAsync(x => x.NameEn == request.NameEn, cancellationToken);
            if (existingAccount)
            {
                _logger.LogError("Account with NameEn {0} already exists", request.NameEn);
                return Result.Fail(new Error(
                    Code: "Account.NameEnExists",
                    Message: $"Account with NameEn {request.NameEn} already exists",
                    Type: ErrorType.Conflict
                    ));
            }
            var pass = _passwordService.Hash(request.Password);
            var account = Account.Create(request.NameEn, request.NameAr, request.Email, request.Password);
            account.AssignRole(new Guid("f37ac10b-58cc-4372-a567-0e02b2c3d479"));
            await _accountRepository.AddAsync(account, cancellationToken);
            await _accountRepository.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}