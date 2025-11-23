using BuildingBlock.Application.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction.Security;
using NPark.Application.Shared.Dto;
using NPark.Domain.Entities;
using System.Diagnostics;
using System.Text.Json;

namespace NPark.Infrastructure.Services
{
    public sealed class AuditLogger : IAuditLogger
    {
        private readonly IGenericRepository<AuditLog> _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenReader _tokenReader;
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(
            IGenericRepository<AuditLog> repo,
            IHttpContextAccessor httpContextAccessor,
            ITokenReader tokenReader,
            ILogger<AuditLogger> logger)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
            _tokenReader = tokenReader;
            _logger = logger;
        }

        public async Task LogAsync(AuditLogEntry entry, CancellationToken ct = default)
        {
            var http = _httpContextAccessor.HttpContext;

            Guid? tokenUserId = null;
            Guid? tokenGateId = null;
            string? tokenRole = null;

            if (http?.User?.Identity?.IsAuthenticated == true)
            {
                var info = _tokenReader.ReadFromPrincipal(http.User);
                tokenUserId = info.UserId;
                tokenGateId = info.GateId;
                tokenRole = info.Role;
            }

            var correlationId = entry.CorrelationId
                ?? http?.Items["__CorrelationId"]?.ToString()
                ?? http?.TraceIdentifier;

            var traceId = entry.TraceId ?? Activity.Current?.Id;

            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,

                EventName = entry.EventName,
                EventCategory = entry.EventCategory,

                IsSuccess = entry.IsSuccess,
                StatusCode = entry.StatusCode,
                ErrorCode = entry.ErrorCode,
                ErrorMessage = entry.ErrorMessage,

                UserId = entry.UserId ?? tokenUserId,
                GateId = entry.GateId ?? tokenGateId,
                Role = entry.Role ?? tokenRole,

                RequestPath = entry.RequestPath ?? http?.Request.Path.Value,
                HttpMethod = entry.HttpMethod ?? http?.Request.Method,

                CorrelationId = correlationId,
                TraceId = traceId,

                ExtraJson = entry.Extra is null
                    ? null
                    : JsonSerializer.Serialize(entry.Extra)
            };

            try
            {
                await _repo.AddAsync(audit, ct);
                await _repo.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for {EventName}", entry.EventName);
            }
        }
    }
}