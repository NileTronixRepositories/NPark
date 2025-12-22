using BuildingBlock.Application.Repositories;
using CRM.Application.Abstraction.Security;
using CRM.Application.Shared.Dto;
using CRM.Domain.Entities;
using CRM.Infrastructure.Option;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRM.Infrastructure.Token
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IGenericRepository<SuperAdmin> _repository;
        private readonly JwtOption _options;
        private readonly CRMDBContext _context;

        public JwtProvider(IGenericRepository<SuperAdmin> repository, IOptions<JwtOption> options, CRMDBContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UserTokenDto> Generate(SuperAdmin user, CancellationToken cancellationToken = default)
        {
            var userEntity = await _context.Set<SuperAdmin>()
                .Where(x => x.Id == user.Id)
                .Include(x => x.Role)
                .ThenInclude(r => r.GetPermissions)
                .ThenInclude(p => p.Permission)
                .AsSplitQuery()
                .AsNoTracking()
                .SingleAsync(cancellationToken);

            if (userEntity == null) throw new Exception("User not found");

            var claims = new List<Claim>
            {
                new("userId", user.Id.ToString()),
                new("email", user.Email ?? ""),
                new("phoneNumber", user.PhoneNumber ?? ""),
            };

            claims.Add(new Claim(ClaimTypes.Role, userEntity.Role.NameEn));

            foreach (var permission in userEntity.Role.GetPermissions)
            {
                claims.Add(new Claim("permission", permission.Permission.NameEn));
            }
            var signingCredentials = new SigningCredentials(
    new SymmetricSecurityKey(

                Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);
            var signingCredential = new SigningCredentials(
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_options.Secret)),
    SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                null,
                DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredential);

            string tokenValue = new JwtSecurityTokenHandler()
                .WriteToken(token);

            var response = new UserTokenDto
            {
                Token = tokenValue,
                RoleName = userEntity.Role.NameEn,
                Permissions = userEntity.Role.GetPermissions.Select(x => x.Permission.NameEn).ToList()
            };
            return response;
        }
    }
}