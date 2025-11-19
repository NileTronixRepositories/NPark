using BuildingBlock.Application.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NPark.Application.Abstraction;
using NPark.Application.Shared.Dto;
using NPark.Application.Specifications.UserSpecification;
using NPark.Domain.Entities;
using NPark.Infrastructure.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NPark.Infrastructure.Services.Token
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IGenericRepository<User> _repository;
        private readonly JwtOption _options;

        public JwtProvider(IGenericRepository<User> repository, IOptions<JwtOption> options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<UserTokenDto> Generate(User user, ParkingGate? gate = null, CancellationToken cancellationToken = default)
        {
            var spec = new GetUserWithPermissionSpecification(user.Id);
            var userEntity = await _repository.FirstOrDefaultWithSpecAsync(spec, cancellationToken);
            if (userEntity == null) throw new Exception("User not found");

            var claims = new List<Claim>
            {
                new("userId", user.Id.ToString()),
                new("userName", user.Name    ?? ""),
                new("email", user.Email ?? ""),
                new("phoneNumber", user.PhoneNumber ?? ""),
            };
            if (gate is not null)
            {
                claims.Add(new Claim("gateId", gate.Id.ToString()));
            }
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