using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PowerGrid.Api.Data;
using PowerGrid.Api.Models;
using PowerGrid.Api.Services;

namespace PowerGrid.Api.Tests;

public class DeviceServiceTests
{
    [Test]
    public async Task AddDeviceAsync_SavesDeviceToDatabase()
    {
        // Create a temporary SQLite database in memory.
        using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        // Configure Entity Framework to use our temporary database.
        var options =
            new DbContextOptionsBuilder<PowerGridDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var context =
            new PowerGridDbContext(options);

        // Create the database structure.
        await context.Database.EnsureCreatedAsync();

        // Create our real DeviceService.
        var service = new DeviceService(context);

        // Create a fake relay for the test.
        var device = new Device
        {
            Name = "Relay-Test",
            Voltage = 122.5,
            Current = 14.2,
            Temperature = 42.0,
            Status = "NORMAL"
        };

        // Use our real service to save the relay.
        var createdDevice =
            await service.AddDeviceAsync(device);

        // Ask the database for the device we just saved.
        var savedDevice =
            await context.Devices.FindAsync(createdDevice.Id);

        // NUnit checks.
        Assert.That(savedDevice, Is.Not.Null);

        Assert.That(
            savedDevice!.Name,
            Is.EqualTo("Relay-Test"));

        Assert.That(
            savedDevice.Voltage,
            Is.EqualTo(122.5));

        Assert.That(
            savedDevice.Current,
            Is.EqualTo(14.2));

        Assert.That(
            savedDevice.Status,
            Is.EqualTo("NORMAL"));
    }


[Test]
public async Task AcknowledgeAlarmAsync_UpdatesAlarmInDatabase()
{
    // Create a temporary SQLite database.
    using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options =
        new DbContextOptionsBuilder<PowerGridDbContext>()
            .UseSqlite(connection)
            .Options;

    await using var context =
        new PowerGridDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new DeviceService(context);

    // Create an alarmed protection relay.
    var device = new Device
    {
        Name = "Relay-Alarm-Test",
        Voltage = 128.0,
        Current = 30.5,
        Temperature = 74.0,
        Status = "ALARM",
        AlarmAcknowledged = false
    };

    // Save it to the temporary database.
    var createdDevice =
        await service.AddDeviceAsync(device);

    // Acknowledge the alarm through DeviceService.
    bool result =
        await service.AcknowledgeAlarmAsync(createdDevice.Id);

    // Read the device from the database.
    var savedDevice =
        await context.Devices.FindAsync(createdDevice.Id);

    // Verify the operation succeeded.
    Assert.That(result, Is.True);

    // Verify the database was actually updated.
    Assert.That(savedDevice, Is.Not.Null);

    Assert.That(
        savedDevice!.AlarmAcknowledged,
        Is.True);
}
[Test]
public async Task AcknowledgeAlarmAsync_NormalDevice_ReturnsFalse()
{
    using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options =
        new DbContextOptionsBuilder<PowerGridDbContext>()
            .UseSqlite(connection)
            .Options;

    await using var context =
        new PowerGridDbContext(options);

    await context.Database.EnsureCreatedAsync();

    var service = new DeviceService(context);

    // Create a NORMAL relay with no active alarm.
    var device = new Device
    {
        Name = "Relay-Normal-Test",
        Voltage = 120.0,
        Current = 10.0,
        Temperature = 40.0,
        Status = "NORMAL",
        AlarmAcknowledged = false
    };

    var createdDevice =
        await service.AddDeviceAsync(device);

    // Try to acknowledge a relay that has no alarm.
    bool result =
        await service.AcknowledgeAlarmAsync(createdDevice.Id);

    // The service should reject the operation.
    Assert.That(result, Is.False);

    var savedDevice =
        await context.Devices.FindAsync(createdDevice.Id);

    Assert.That(savedDevice, Is.Not.Null);

    // It should remain unacknowledged.
    Assert.That(
        savedDevice!.AlarmAcknowledged,
        Is.False);
}
}
