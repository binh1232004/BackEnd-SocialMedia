using Application.Hubs;
using DotNetEnv;
using SMedia.Configuration;
using Serilog;
using SMedia.Extensions;
using Serilog.Events;
Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Hỗ trợ Unicode
    });

// Tắt tất cả logger mặc định
builder.Logging.ClearProviders();

// Cấu hình Serilog
// builder.Host.UseSerilog((context, configuration) =>
// {
//     configuration
//         .WriteTo.Console(
//             outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
//         .WriteTo.File("logs/log-.txt",
//             rollingInterval: RollingInterval.Day,
//             outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
//         .MinimumLevel.Information()
//         .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Fatal)
//         .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Fatal)
//         .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
//         .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Fatal)
//         .Enrich.FromLogContext();
// });
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Debug)
        .MinimumLevel.Override("System.Text.Json", Serilog.Events.LogEventLevel.Verbose)
        .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", Serilog.Events.LogEventLevel.Debug)
        .MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections", Serilog.Events.LogEventLevel.Debug)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext();
});

// Tắt log EF Core chi tiết
// builder.Services.AddLogging(logging =>
// {
//     logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
//     logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
//     logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug);
//     logging.AddFilter("System", LogLevel.None);
// });

builder.Services.AddLogging(logging =>
{
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Information);
    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug); // Cho phép log Debug
    logging.AddFilter("System", LogLevel.Information);
});

builder.Services.AddApplicationServices();

//Cho phép website khác truy cập vào API
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend",
//         policy =>
//         {
//             policy.WithOrigins(Env.GetString("FQDN_FRONTEND"))
//                   .AllowAnyHeader()
//                   .AllowAnyMethod();
//         });
// });

// Thêm SignalR
// builder.Services.AddSignalR();

// Thêm SignalR và tối ưu WebSocket
builder.Services.AddSignalR(options =>
    {
        // Các cấu hình chung cho SignalR
        options.EnableDetailedErrors = true; // Hiển thị lỗi chi tiết để debug
        options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Giữ kết nối WebSocket
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Tăng thời gian chờ client
        options.HandshakeTimeout = TimeSpan.FromSeconds(30); // Tăng thời gian chờ handshake
        options.MaximumReceiveMessageSize = 1024000; // Tăng giới hạn kích thước tin nhắn
    })
    .AddHubOptions<ChatHub>(options =>
    {
        // Cấu hình cho ChatHub
        options.SupportedProtocols = new List<string> { "json" }; // Giao thức truyền dữ liệu
    });


// Cập nhật CORS để hỗ trợ SignalR (thêm AllowCredentials)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // policy.WithOrigins(Env.GetString("FQDN_FRONTEND"))
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Thêm để hỗ trợ SignalR
        });
});

var app = builder.Build();

// Đặt middleware CORS trước tất cả middleware khác
app.UseCors("AllowFrontend");

// Middleware để bỏ qua xác thực cho yêu cầu OPTIONS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204; // No Content
        context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:3000");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization,Content-Type");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        return;
    }
    await next.Invoke();
});


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

app.UseCustomHttpLogging();

// Đăng ký middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
// Map SignalR Hub
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllers();

app.Run();