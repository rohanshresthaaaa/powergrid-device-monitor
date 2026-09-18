using Microsoft.EntityFrameworkCore;
using PowerGrid.Api.Data;
using PowerGrid.Api.Models;
using PowerGrid.Api.Services;

var builder = WebApplication.CreateBuilder(args);


// -------------------------------------------------------
// DATABASE
// -------------------------------------------------------

// Connect Entity Framework Core to our SQLite database.
// The connection string comes from appsettings.json.
builder.Services.AddDbContext<PowerGridDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("PowerGridDatabase")
    )
);


// -------------------------------------------------------
// SERVICES
// -------------------------------------------------------

// DeviceService handles device-related business logic.
// Scoped means a new service instance is created for each HTTP request.
builder.Services.AddScoped<DeviceService>();


// -------------------------------------------------------
// CORS
// -------------------------------------------------------

// Allow our React frontend to communicate with this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();
// -------------------------------------------------------
// INITIAL DATABASE DATA
// -------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<PowerGridDbContext>();

    if (!await dbContext.Devices.AnyAsync())
    {
        dbContext.Devices.AddRange(
            new Device
            {
                Name = "Relay-101",
                Voltage = 120.4,
                Current = 12.1,
                Temperature = 41.0,
                Status = "NORMAL",
                AlarmAcknowledged = false
            },

            new Device
            {
                Name = "Relay-102",
                Voltage = 118.9,
                Current = 18.7,
                Temperature = 46.0,
                Status = "NORMAL",
                AlarmAcknowledged = false
            },

            new Device
            {
                Name = "Relay-103",
                Voltage = 126.2,
                Current = 29.4,
                Temperature = 71.0,
                Status = "ALARM",
                AlarmAcknowledged = false
            }
        );

        await dbContext.SaveChangesAsync();
    }
}

// -------------------------------------------------------
// MIDDLEWARE
// -------------------------------------------------------

app.UseCors("Frontend");


// -------------------------------------------------------
// BASIC API TEST
// -------------------------------------------------------

app.MapGet("/", () =>
    "PowerGrid Device Monitor API is running");


// -------------------------------------------------------
// GET ALL DEVICES
// -------------------------------------------------------

app.MapGet("/api/devices",
    async (DeviceService service) =>
{
    var devices = await service.GetDevicesAsync();

    return Results.Ok(devices);
});


// -------------------------------------------------------
// GET ONE DEVICE
// -------------------------------------------------------

app.MapGet("/api/devices/{id:int}",
    async (int id, DeviceService service) =>
{
    var device = await service.GetDeviceAsync(id);

    return device is null
        ? Results.NotFound(
            new { message = "Device not found." })
        : Results.Ok(device);
});


// -------------------------------------------------------
// ACKNOWLEDGE AN ALARM
// -------------------------------------------------------

app.MapPost(
    "/api/devices/{id:int}/acknowledge",
    async (int id, DeviceService service) =>
{
    bool success =
        await service.AcknowledgeAlarmAsync(id);

    return success
        ? Results.Ok(
            new { message = "Alarm acknowledged." })
        : Results.NotFound(
            new { message = "Active alarm not found." });
});


// -------------------------------------------------------
// ADD A NEW DEVICE
// -------------------------------------------------------

app.MapPost("/api/devices",
    async (Device device, DeviceService service) =>
{
    // Device must have a name.
    if (string.IsNullOrWhiteSpace(device.Name))
    {
        return Results.BadRequest(
            new { message = "Device name is required." });
    }

    // Electrical measurements cannot be negative.
    if (device.Voltage < 0 ||
        device.Current < 0 ||
        device.Temperature < 0)
    {
        return Results.BadRequest(
            new
            {
                message =
                    "Measurements cannot be negative."
            });
    }

    // Only allow the two statuses used by our system.
    if (device.Status != "NORMAL" &&
        device.Status != "ALARM")
    {
        return Results.BadRequest(
            new
            {
                message =
                    "Status must be NORMAL or ALARM."
            });
    }

    // Save the new device to SQLite.
    var createdDevice =
        await service.AddDeviceAsync(device);

    return Results.Created(
        $"/api/devices/{createdDevice.Id}",
        createdDevice);
});


app.Run();