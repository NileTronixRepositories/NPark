using BuildingBlock.Api.Bootstrap;
using BuildingBlock.Api.Logging;
using BuildingBlock.Api.OpenAi;
using CRM.Application.BootStrap;
using CRM.Infrastructure.Bootstrap;
using CRM.Infrastructure.Option;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationBootstrap();
builder.Services.InfrastructureInjection(builder.Configuration);
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
builder.AddSerilogBootstrap("CRM.Api");
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
    options.AddPolicy("CRM", policy =>
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

var app = builder.Build();

app.UseSerilogPipeline();

// HTTPS
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("CRM");

app.UseAuthentication();
app.UseAuthorization();

app.UseSharedLocalization();

// Swagger (اختياري حسب الـ Env)
app.UseSwagger();
app.UseSwaggerUI();

// Custom Middleware
app.MapLoggingDiagnostics();

// Controllers
app.MapControllers();

app.Run();