using DotNetEnv;
using SMedia.Configuration;
using Serilog;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.Console() // Ghi log ra console
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext();
});

builder.Services.AddApplicationServices();

builder.Services.AddWebSocketServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Gọi CORS trước để cho phép FE truy cập

app.UseWebSockets(); // Kích hoạt WebSocket middleware
app.UseWebSocketHandler(); // Xử lý các yêu cầu WebSocket tại /ws

app.UseSerilogRequestLogging(); // Log các yêu cầu HTTP

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();