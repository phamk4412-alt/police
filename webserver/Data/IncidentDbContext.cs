using Microsoft.EntityFrameworkCore;
using PoliceWebServer.Models;

namespace PoliceWebServer.Data;

public sealed class IncidentDbContext(DbContextOptions<IncidentDbContext> options) : DbContext(options)
{
    public DbSet<IncidentRecord> Incidents => Set<IncidentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var incident = modelBuilder.Entity<IncidentRecord>();
        incident.ToTable("Incidents");
        incident.HasKey(item => item.Id);

        incident.Property(item => item.Title)
            .HasMaxLength(160)
            .IsRequired();

        incident.Property(item => item.Detail)
            .HasMaxLength(4000)
            .IsRequired();

        incident.Property(item => item.Level)
            .HasMaxLength(24)
            .IsRequired();

        incident.Property(item => item.TimeLabel)
            .HasMaxLength(16)
            .IsRequired();

        incident.Property(item => item.Status)
            .HasMaxLength(64)
            .IsRequired();

        incident.Property(item => item.Source)
            .HasMaxLength(32)
            .IsRequired();

        incident.HasIndex(item => item.CreatedAt);
        incident.HasIndex(item => item.Status);
    }
}
