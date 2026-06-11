using HomeSync.WebAPI.BackgroundServices;
using HomeSync.WebAPI.Consumers;
using HomeSync.WebAPI.Helpers;
using HomeSync.WebAPI.Hubs;
using HomeSync.WebAPI.Interfaces;
using HomeSync.WebAPI.Middlewares;
using HomeSync.WebAPI.Models.Settings;
using HomeSync.WebAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddOptions<ClientSettings>().BindConfiguration("Client").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<DemoUserSettings>().BindConfiguration("DemoUser").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<JwtSettings>().BindConfiguration("Jwt").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<RabbitMQSettings>().BindConfiguration("RabbitMQ").ValidateDataAnnotations().ValidateOnStart();

Environment.SetEnvironmentVariable("MT_LICENSE", "open-source");
builder.Services.AddMassTransit(x =>
{
    var rabbitMQSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>() ?? throw new InvalidOperationException("RabbitMQSettings configuration is missing.");
    x.AddConsumer<SensorDataConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.SetLicense("community");
        var rabbitUrl = rabbitMQSettings.ConnectionString;
        cfg.Host(new Uri(rabbitUrl));

        cfg.ReceiveEndpoint("home-sync-sensor-queue", e =>
        {
            e.ConfigureConsumer<SensorDataConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
        });
    });
});

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<SensorDataSimulatorWorker>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SensorDataSimulatorWorker>());

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddHealthChecks();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings configuration is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var rsaKey = RsaKeyLoader.LoadPublicKey(jwtSettings.PublicKeyPath);
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = rsaKey,

            NameClaimType = JwtRegisteredClaimNames.Email
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/sensorHub"))
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

var clientSettings = builder.Configuration.GetSection("Client").Get<ClientSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (clientSettings != null && !string.IsNullOrEmpty(clientSettings.BaseUrl))
        {
            policy.WithOrigins(clientSettings.BaseUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseMiddleware<RequestMetadataMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthorization();

app.MapHub<SensorHub>("/sensorHub");
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();