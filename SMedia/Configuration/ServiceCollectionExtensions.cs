using Application.DTOs;
using Mapster;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using DotNetEnv; 

namespace SMedia.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Thêm controllers
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Cấu hình Swagger
        services.AddSwaggerConfiguration();

        // Cấu hình xác thực JWT
        services.AddJwtAuthentication();

        // Thêm phân quyền
        services.AddAuthorization();

        // Cấu hình DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Env.GetString("CONNECTION_STRING")));

        // Thêm bộ nhớ cache
        services.AddMemoryCache();

        // Cấu hình email
        services.AddEmailConfiguration();

        // Đăng ký các dịch vụ
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMessageService, MessageService>();
            
        // Đăng ký các kho lưu trữ
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Thêm Mapster
        services.AddMapster();
        MapsterConfig.RegisterMappings();

        return services;
    }
}