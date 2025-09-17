using LarkAutoTrainCert.Helpers;
using LarkAutoTrainCert.Model;
using LarkAutoTrainCert.Service;
using Serilog;

// Configure Serilog with File Sink
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Log to file
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register LarkSettings with DI
builder.Services.Configure<LarkSettings>(builder.Configuration.GetSection("LarkSettings"));
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));
builder.Services.Configure<LoginModel>(builder.Configuration.GetSection("LoginModel"));
builder.Services.Configure<SQLModel>(builder.Configuration.GetSection("Database"));


// Register the services
builder.Services.AddSingleton<LarkService>();
builder.Services.AddSingleton<SAPService>();
builder.Services.AddSingleton<MSSQLHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger(); // Enable Swagger in production
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty; // Set to empty if you want Swagger at the root URL
    });
}
app.MapGet("/api/ping", () => "pong");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
