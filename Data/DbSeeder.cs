using Microsoft.EntityFrameworkCore;
using PractoBackend.Data;
using PractoBackend.Enums;
using PractoBackend.Models;

namespace PractoBackend
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Clinics
            var clinics = new[]
            {
                new Clinic { Name = "Kadavanthra Skin & Hair Laser Clinic", Address = "Kadavanthra", City = "Ernakulam", ContactNumber = "08037296159" },
                new Clinic { Name = "Almeka Medical Centre", Address = "Palarivattom", City = "Ernakulam", ContactNumber = "08012345678" }
            };

            foreach (var clinic in clinics)
            {
                if (!context.Clinics.Any(c => c.Name == clinic.Name))
                {
                    context.Clinics.Add(clinic);
                }
            }
            context.SaveChanges();

            // Seed Calendar Categories
            if (!context.CalendarCategories.Any())
            {
                var categories = new[] { "Operation Theatre 1", "Operation Theatre 2", "General OPD", "Virtual Connect" };
                foreach (var cat in categories)
                {
                    context.CalendarCategories.Add(new CalendarCategory { Name = cat });
                }
            }
            context.SaveChanges();

            // Seed Specializations
            var specializations = new[] { "Dentist", "Gynecologist/obstetrician", "General Physician", "Dermatologist", "Ear-nose-throat (ent) Specialist", "Homoeopath", "Ayurveda", "Sexology", "Psychiatry" };
            foreach (var spec in specializations)
            {
                if (!context.Specializations.Any(s => s.Name == spec))
                {
                    context.Specializations.Add(new Specialization { Name = spec, Description = spec });
                }
            }
            context.SaveChanges();

            // Seed Doctors (Screenshot matches)
            SeedDoctor(context, "Dr. Annu Jayan", "Dermatologist", "annu@practo.local", "08037296159", 25, 400, "Kadavanthra Skin & Hair Laser Clinic", false);
            SeedDoctor(context, "Dr. Celia Mathew", "Dermatologist", "celia@practo.local", "08012345678", 20, 500, "Almeka Medical Centre", false);
            SeedDoctor(context, "Dr. Soumya", "Dermatologist", "soumya@practo.local", "08099998888", 15, 300, "Kadavanthra Skin & Hair Laser Clinic", false);
            
            // A couple more for other specialities
            SeedDoctor(context, "Dr. Ramesh Dentist", "Dentist", "ramesh@practo.local", "0801112222", 10, 600, "Almeka Medical Centre", false);
            SeedDoctor(context, "Dr. Anita Gyno", "Gynecologist/obstetrician", "anita@practo.local", "0803334444", 12, 800, "Almeka Medical Centre", false);

            // Video Consult Doctors
            SeedDoctor(context, "Dr. Video Gyno", "Gynecologist/obstetrician", "vgyno@practo.local", "0801231231", 8, 199, "Almeka Medical Centre", true);
            SeedDoctor(context, "Dr. Video Sexology", "Sexology", "vsex@practo.local", "0801231232", 15, 299, "Kadavanthra Skin & Hair Laser Clinic", true);
            SeedDoctor(context, "Dr. Video GP", "General Physician", "vgp@practo.local", "0801231233", 20, 199, "Almeka Medical Centre", true);
            SeedDoctor(context, "Dr. Video Derm", "Dermatologist", "vderm@practo.local", "0801231234", 10, 249, "Kadavanthra Skin & Hair Laser Clinic", true);
            SeedDoctor(context, "Dr. Video Psych", "Psychiatry", "vpsych@practo.local", "0801231235", 5, 299, "Almeka Medical Centre", true);

            // Seed Lab Tests
            var labTests = new[]
            {
                new LabTest { Name = "Thyroid Profile", Description = "Thyroid Profile Total Blood", Price = 420, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 },
                new LabTest { Name = "Complete Blood Count", Description = "Complete Blood Count Automated Blood", Price = 330, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 },
                new LabTest { Name = "Lipid Profile", Description = "Lipid Profile Blood", Price = 620, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 },
                new LabTest { Name = "Liver Function Test", Description = "Liver Function Tests Blood", Price = 790, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 },
                new LabTest { Name = "Dengue NS 1", Description = "Dengue Ns1 Antigen Pcr Blood", Price = 630, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 },
                new LabTest { Name = "Malarial Antigen", Description = "Malarial Antigen Pcr Blood", Price = 680, OriginalPrice = null, ReportTime = "E-Reports on next day", Category = "Individual", TestCount = 1 }
            };

            foreach (var test in labTests)
            {
                if (!context.LabTests.Any(lt => lt.Name == test.Name))
                {
                    context.LabTests.Add(test);
                }
            }
            context.SaveChanges();

            // Seed Surgery Types
            var surgeries = new[]
            {
                new SurgeryType { Name = "Cataract Surgery", Category = "Ophthalmology", Description = "Removal of the natural lens of the eye", EstimatedCost = 35000 },
                new SurgeryType { Name = "LASIK", Category = "Ophthalmology", Description = "Laser eye surgery to correct vision", EstimatedCost = 45000 },
                new SurgeryType { Name = "Knee Replacement", Category = "Orthopedics", Description = "Surgical procedure to replace the weight-bearing surfaces of the knee joint", EstimatedCost = 150000 },
                new SurgeryType { Name = "ACL Reconstruction", Category = "Orthopedics", Description = "Surgical tissue replacement of the anterior cruciate ligament", EstimatedCost = 80000 },
                new SurgeryType { Name = "Tonsillectomy", Category = "ENT", Description = "Surgical removal of the tonsils", EstimatedCost = 40000 },
                new SurgeryType { Name = "Rhinoplasty", Category = "ENT", Description = "Plastic surgery procedure for altering and reconstructing the nose", EstimatedCost = 75000 }
            };

            foreach (var surgery in surgeries)
            {
                if (!context.SurgeryTypes.Any(st => st.Name == surgery.Name))
                {
                    context.SurgeryTypes.Add(surgery);
                }
            }
            context.SaveChanges();
        }

        private static void SeedDoctor(ApplicationDbContext context, string name, string specName, string email, string phone, int exp, decimal fee, string clinicName, bool isVideoConsult)
        {
            if (context.Users.Any(u => u.Email == email)) return;

            var user = new User
            {
                FullName = name,
                Email = email,
                PasswordHash = "password",
                PhoneNumber = phone,
                Role = Role.Doctor,
                IsActive = true,
                EmailVerified = true
            };
            context.Users.Add(user);
            context.SaveChanges();

            var docProfile = new DoctorProfile
            {
                UserId = user.Id,
                ConsultationFee = fee,
                ExperienceYears = exp,
                IsVerified = true,
                Biography = "Experienced professional",
                Qualifications = "MBBS, MD",
                IsVideoConsult = isVideoConsult
            };
            context.DoctorProfiles.Add(docProfile);
            context.SaveChanges();

            var spec = context.Specializations.First(s => s.Name == specName);
            context.DoctorSpecializations.Add(new DoctorSpecialization { DoctorId = docProfile.Id, SpecializationId = spec.Id });

            var clinic = context.Clinics.First(c => c.Name == clinicName);
            context.DoctorClinics.Add(new DoctorClinic { DoctorId = docProfile.Id, ClinicId = clinic.Id });

            // Generate slots for Today and Tomorrow
            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);

            // Morning slots
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(11, 15, 0), EndTime = new TimeSpan(11, 30, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(11, 30, 0), EndTime = new TimeSpan(11, 45, 0), IsBooked = false });
            
            // Afternoon slots
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(12, 15, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(12, 45, 0), EndTime = new TimeSpan(13, 0, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(13, 45, 0), EndTime = new TimeSpan(14, 0, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 15, 0), IsBooked = false });

            // Evening slots
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(16, 30, 0), EndTime = new TimeSpan(16, 45, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(16, 45, 0), EndTime = new TimeSpan(17, 0, 0), IsBooked = false });
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = today, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(18, 15, 0), IsBooked = false });

            // Slots for tomorrow
            context.AvailabilitySlots.Add(new AvailabilitySlot { DoctorId = docProfile.Id, ClinicId = clinic.Id, SlotDate = tomorrow, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 15, 0), IsBooked = false });

            context.SaveChanges();
        }
    }
}
