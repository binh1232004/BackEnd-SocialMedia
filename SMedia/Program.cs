using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.DTOs;
using DotNetEnv;
using Infrastructure.Data;
using Application.Interfaces.ServiceInterfaces;
using Application.Services;
using Application.Interfaces.RepositoryInterfaces;
using Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Http;
using SMedia.Extensions;
using Swashbuckle.AspNetCore.Filters;
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add logging services
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Social Media API",
        Description = "A social media backend API built with ASP.NET Core",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@socialmedia.example.com"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    options.DocInclusionPredicate((_, _) => true);
    options.CustomOperationIds(e => e.ActionDescriptor.RouteValues["action"]);
    options.OperationFilter<RemoveDefaultResponse>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token JWT: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });    // Enable Swagger examples
    options.ExampleFilters();
    
    // Customize Swagger UI to better display examples
    options.UseInlineDefinitionsForEnums();
});

// Đọc cấu hình JWT từ .env
var jwtConfig = new JwtConfiguration
{
    Issuer = Env.GetString("JWT_ISSUER") ?? "smedia-issuer", // Default fallback if not in .env
    Audience = Env.GetString("JWT_AUDIENCE") ?? "smedia-users", // Default fallback if not in .env, match the token
    Key = Env.GetString("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY is not set in .env")
};

// Đăng ký JwtConfiguration như một singleton
builder.Services.AddSingleton(jwtConfig);

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {        // Very lenient token validation for development environment
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Skip issuer validation for troubleshooting
            ValidateAudience = false, // Skip audience validation for troubleshooting
            ValidateLifetime = false, // Ignore token expiration in development
            ValidateIssuerSigningKey = false, // Skip signature validation for troubleshooting
            RequireSignedTokens = false, // Allow tokens without signature for troubleshooting
            
            // These values are still here but not used when validation is disabled
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
            
            // Accept any token for debugging purposes
            SignatureValidator = (token, parameters) => 
            {
                var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
                return handler.ReadJsonWebToken(token);
            }
        };
        
        // Add event handlers to debug token validation
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token was validated successfully!");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"Token received: {context.Token?.Substring(0, Math.Min(context.Token?.Length ?? 0, 20))}...");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(Env.GetString("CONNECTION_STRING")));

builder.Services.AddMemoryCache();

// Đọc cấu hình email từ .env
var emailConfig = new EmailConfiguration
{
    SmtpServer = Env.GetString("EMAIL_SMTP_SERVER") ?? throw new InvalidOperationException("EMAIL_SMTP_SERVER is not set in .env"),
    Port = int.TryParse(Env.GetString("EMAIL_PORT"), out var port) ? port : throw new InvalidOperationException("EMAIL_PORT is not set or invalid in .env"),
    SenderEmail = Env.GetString("EMAIL_SENDER_EMAIL") ?? throw new InvalidOperationException("EMAIL_SENDER_EMAIL is not set in .env"),
    Password = Env.GetString("EMAIL_PASSWORD") ?? throw new InvalidOperationException("EMAIL_PASSWORD is not set in .env"),
    SenderName = Env.GetString("EMAIL_SENDER_NAME") ?? throw new InvalidOperationException("EMAIL_SENDER_NAME is not set in .env")
};

// Đăng ký EmailConfiguration như một singleton
builder.Services.AddSingleton(emailConfig);

// Đăng ký Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Đăng ký Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// Thêm dịch vụ Mapster
builder.Services.AddMapster();
// Gọi cấu hình ánh xạ
MapsterConfig.RegisterMappings();

// Register examples for Swagger
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
// -------------------------------------------------- Bình -------------------------------------------------- //
//Cho phép website khác truy cập vào API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(Env.GetString("FQDN_FRONTEND"))
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add HTTP request logging middleware
var app = builder.Build();
app.UseCustomHttpLogging();

// -------------------------------------------------- Bình -------------------------------------------------- //

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SMedia API V1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "SMedia API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
        options.DisplayRequestDuration();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse operations by default
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();