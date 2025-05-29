namespace EnvironmentsService.Src.Infraestructure.Data;

using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Environment> Environments { get; set; }

    public DbSet<EnvironmentType> EnvironmentTypes { get; set; }

    public DbSet<PricingPolicy> PricingPolicies { get; set; }

    public DbSet<DiscountPolicy> DiscountPolicies { get; set; }

    public DbSet<NonAvailability> NonAvailabilities { get; set; }

    public DbSet<EnvironmentPhoto> EnvironmentPhotos { get; set; }

    public DbSet<Service> Services { get; set; }

    public DbSet<Area> Areas { get; set; }

    public DbSet<EnvironmentService> EnvironmentServices { get; set; }

    public DbSet<EnvironmentArea> EnvironmentAreas { get; set; }

    public DbSet<WeeklySchedule> WeeklySchedules { get; set; }

    public DbSet<SpecialAvailability> SpecialAvailabilities { get; set; }

    public DbSet<Reservation> Reservations { get; set; }

    public DbSet<ReservationPayment> ReservationPayments { get; set; }

    public DbSet<ReservationTimeRange> ReservationTimeRanges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EnvironmentType - Environment (1:N)
        modelBuilder.Entity<Environment>()
            .HasOne(e => e.Type)
            .WithMany()
            .HasForeignKey(e => e.TypeId);

        // PricingPolicy - Environment (1:N)
        modelBuilder.Entity<PricingPolicy>()
            .HasOne(p => p.Environment)
            .WithMany(e => e.PricingPolicies)
            .HasForeignKey(p => p.EnvironmentId);

        // DiscountPolicy - Environment (1:N)
        modelBuilder.Entity<DiscountPolicy>()
            .HasOne(d => d.Environment)
            .WithMany(e => e.DiscountPolicies)
            .HasForeignKey(d => d.EnvironmentId);

        // NonAvailability - Environment (1:N)
        modelBuilder.Entity<NonAvailability>()
            .HasOne(n => n.Environment)
            .WithMany(e => e.NonAvailabilities)
            .HasForeignKey(n => n.EnvironmentId);

        // WeeklySchedule - Environment (1:N)
        modelBuilder.Entity<WeeklySchedule>()
            .HasOne(w => w.Environment)
            .WithMany(e => e.WeeklySchedules)
            .HasForeignKey(w => w.EnvironmentId);

        // SpecialAvailability - Environment (1:N)
        modelBuilder.Entity<SpecialAvailability>()
            .HasOne(s => s.Environment)
            .WithMany(e => e.SpecialAvailabilities)
            .HasForeignKey(s => s.EnvironmentId);

        // EnvironmentService - Environment & Service (M:N)
        modelBuilder.Entity<EnvironmentService>()
            .HasKey(es => new { es.EnvironmentId, es.ServiceId });

        modelBuilder.Entity<EnvironmentService>()
            .HasOne(es => es.Environment)
            .WithMany(e => e.EnvironmentServices)
            .HasForeignKey(es => es.EnvironmentId);

        modelBuilder.Entity<EnvironmentService>()
            .HasOne(es => es.Service)
            .WithMany(s => s.EnvironmentServices)
            .HasForeignKey(es => es.ServiceId);

        // EnvironmentArea - Environment & Area (M:N)
        modelBuilder.Entity<EnvironmentArea>()
            .HasKey(ea => new { ea.EnvironmentId, ea.AreaId });

        modelBuilder.Entity<EnvironmentArea>()
            .HasOne(ea => ea.Environment)
            .WithMany(e => e.EnvironmentAreas)
            .HasForeignKey(ea => ea.EnvironmentId);

        modelBuilder.Entity<EnvironmentArea>()
            .HasOne(ea => ea.Area)
            .WithMany(a => a.EnvironmentAreas)
            .HasForeignKey(ea => ea.AreaId);

        // Reservation - Environment (1:N)
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Environment)
            .WithMany(e => e.Reservations)
            .HasForeignKey(r => r.EnvironmentId);

        // Reservation - ReservationPayments (1:N)
        modelBuilder.Entity<ReservationPayment>()
            .HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationRequestId);

        // Reservation - NonAvailability (1:N) → created blockings per reservation
        modelBuilder.Entity<NonAvailability>()
            .HasOne(n => n.Reservation)
            .WithMany(r => r.BlockedSlots)
            .HasForeignKey(n => n.ReservationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Reservation - ReservationTimeRange (1:N)
        modelBuilder.Entity<ReservationTimeRange>()
            .HasOne(rtr => rtr.Reservation)
            .WithMany(r => r.TimeRanges)
            .HasForeignKey(rtr => rtr.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
