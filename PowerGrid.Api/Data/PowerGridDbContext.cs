using Microsoft.EntityFrameworkCore;
using PowerGrid.Api.Models;

namespace PowerGrid.Api.Data;

public class PowerGridDbContext : DbContext
{
    public PowerGridDbContext(
        DbContextOptions<PowerGridDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
}