using Microsoft.EntityFrameworkCore;
using PowerGrid.Api.Data;
using PowerGrid.Api.Models;

namespace PowerGrid.Api.Services;

public class DeviceService
{
    private readonly PowerGridDbContext _context;

    public DeviceService(PowerGridDbContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _context.Devices
            .OrderBy(device => device.Id)
            .ToListAsync();
    }

    public async Task<Device?> GetDeviceAsync(int id)
    {
        return await _context.Devices.FindAsync(id);
    }

    public async Task<bool> AcknowledgeAlarmAsync(int id)
    {
        Device? device = await _context.Devices.FindAsync(id);

        if (device == null || device.Status != "ALARM")
        {
            return false;
        }

        device.AlarmAcknowledged = true;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Device> AddDeviceAsync(Device device)
    {
        device.Id = 0;
        device.AlarmAcknowledged = false;

        _context.Devices.Add(device);

        await _context.SaveChangesAsync();

        return device;
    }
}