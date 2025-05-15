using DotNetEnv;
using SMedia.Configuration;
using Serilog;
using SMedia.Extensions;
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

var app = builder.Build();

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
app.UseWebSockets(); // Kích hoạt WebSocket middleware
app.UseWebSocketHandler(); // Xử lý các yêu cầu WebSocket tại /ws

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();