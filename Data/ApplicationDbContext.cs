using Microsoft.EntityFrameworkCore;
using PractoBackend.Models;

namespace PractoBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PatientProfile> PatientProfiles { get; set; } = null!;
    public DbSet<DoctorProfile> DoctorProfiles { get; set; } = null!;
    public DbSet<Specialization> Specializations { get; set; } = null!;
    public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; } = null!;
    public DbSet<Clinic> Clinics { get; set; } = null!;
    public DbSet<DoctorClinic> DoctorClinics { get; set; } = null!;
    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<CalendarCategory> CalendarCategories { get; set; } = null!;
    
    public DbSet<LabTest> LabTests { get; set; } = null!;
    public DbSet<LabTestBooking> LabTestBookings { get; set; } = null!;
    public DbSet<SurgeryType> SurgeryTypes { get; set; } = null!;
    public DbSet<SurgeryEnquiry> SurgeryEnquiries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite Keys
        modelBuilder.Entity<DoctorSpecialization>()
            .HasKey(ds => new { ds.DoctorId, ds.SpecializationId });

        modelBuilder.Entity<DoctorClinic>()
            .HasKey(dc => new { dc.DoctorId, dc.ClinicId });

        // Concurrency Token
        modelBuilder.Entity<AvailabilitySlot>()
            .Property(a => a.RowVersion)
            .IsRowVersion();

        // Unique Constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<PatientProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<DoctorProfile>()
            .HasIndex(d => d.UserId)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.SlotId)
            .IsUnique();

        modelBuilder.Entity<MedicalRecord>()
            .HasIndex(m => m.AppointmentId)
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => r.AppointmentId)
            .IsUnique();

        // Cascade Delete Restrictions
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Slot)
            .WithMany()
            .HasForeignKey(a => a.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Patient)
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Doctor)
            .WithMany()
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Doctor)
            .WithMany()
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
