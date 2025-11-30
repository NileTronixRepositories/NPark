using BuildingBlock.Api.Bootstrap;
using BuildingBlock.Api.Logging;
using BuildingBlock.Api.OpenAi;
using Microsoft.OpenApi.Models;
using NPark.Api.RealTime;
using NPark.Application.Abstraction;
using NPark.Application.Bootstrap;
using NPark.Infrastructure.Bootstrap;
using NPark.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InfrastructureInjection(builder.Configuration);
builder.Services.AddApplicationBootstrap();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

builder.Services.AddSwaggerGen(c =>
{
    // Add Bearer Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the Bearer token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.OperationFilter<ResultPatternOperationFilter>();
});
// ---------Serilog-------- /
builder.AddSerilogBootstrap("NPARK.Api");
//---------Localization-------- /
builder.Services.AddSharedLocalization(opts =>
{
    opts.SupportedCultures = new[] { "ar", "en" };
    opts.DefaultCulture = "en";
    opts.AllowQueryStringLang = true;
});
builder.Services.AddMemoryCache();
var corsSettings = builder.Configuration
    .GetSection("Cors")
    .Get<CorsSettings>() ?? new CorsSettings();
builder.Services.AddCors(options =>
{
    options.AddPolicy("NParkCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (corsSettings.AllowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            if (corsSettings.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowCredentials();
            }
            else
            {
                // fallback آمن نسبياً: مفيش origins → معناه block للـ cross-origin
                policy.WithOrigins(Array.Empty<string>());
            }
        }
    });
});

builder.Services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();
var app = builder.Build();
app.UseSerilogPipeline();
app.UseCors("NParkCors");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseSharedLocalization();
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

// Custom Middleware
app.MapLoggingDiagnostics();
///////////////////////
app.MapControllers();
app.Run();