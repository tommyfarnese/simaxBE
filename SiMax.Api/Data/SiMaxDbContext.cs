using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Models;

namespace SiMax.Api.Data
{
    public class SiMaxDbContext : IdentityDbContext
    {
        public SiMaxDbContext(DbContextOptions<SiMaxDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events => Set<Event>();

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Tournament> Tournaments => Set<Tournament>();

        public DbSet<Registration> Registrations => Set<Registration>();

        public DbSet<Pool> Pools => Set<Pool>();

        public DbSet<PoolEntry> PoolEntries => Set<PoolEntry>();

        public DbSet<Court> Courts => Set<Court>();

        public DbSet<CourtAvailability> CourtAvailabilities
            => Set<CourtAvailability>();

        public DbSet<Match> Matches => Set<Match>();

        public DbSet<TournamentCourt> TournamentCourts => Set<TournamentCourt>();

        public DbSet<FinalPhase> FinalPhases => Set<FinalPhase>();

        public DbSet<FinalPhaseQualification> FinalPhaseQualifications
            => Set<FinalPhaseQualification>();

        public DbSet<FinalMatch> FinalMatches => Set<FinalMatch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tournaments)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tournaments)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tournament>()
                .Property(t => t.Price)
                .HasColumnType("int");

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Tournament)
                .WithMany()
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Registration)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Maschile"
                },
                new Category
                {
                    Id = 2,
                    Name = "Femminile"
                },
                new Category
                {
                    Id = 3,
                    Name = "MIX"
                }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    Id = 1,
                    Title = "Torneo di Fine Estate",
                    Date = new DateTime(2026, 10, 10),
                    Location = "PalaUno",
                    Address = "Largo Antonio Balestra 5, Milano",
                    PeriodLabel = "Mattina & Pomeriggio"
                }
            );

            modelBuilder.Entity<Tournament>().HasData(
                new Tournament
                {
                    Id = 1,
                    EventId = 1,
                    CategoryId = 1,
                    SortOrder = 1,
                    TabLabel = "2×2 MM",
                    Title = "Torneo di Fine Estate MM",
                    Format = "2×2",
                    StartTime = new TimeSpan(9, 30, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    Price = 25,
                    MaxTeams = 10,
                    Level = "Open",
                    MinPlayers = 2,
                    FormUrl = "https://docs.google.com/forms/d/e/1FAIpQLSceRph3nNrHEA_c-SrICg_OuU23-RkOtSAd2ft9s7TXhpOqkQ/viewform?usp=header",
                    WaitlistFormUrl = ""
                },
                new Tournament
                {
                    Id = 2,
                    EventId = 1,
                    CategoryId = 2,
                    SortOrder = 2,
                    TabLabel = "2×2 FF",
                    Title = "Torneo di Fine Estate FF",
                    Format = "2×2",
                    StartTime = new TimeSpan(9, 30, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    Price = 25,
                    MaxTeams = 10,
                    Level = "Open",
                    MinPlayers = 2,
                    FormUrl = "https://docs.google.com/forms/d/e/1FAIpQLSdQvcEcQxF_3ouLmuEWYggWa9Otg2L8MD92jHy7DwKaU1clcA/viewform?usp=header",
                    WaitlistFormUrl = ""
                },
                new Tournament
                {
                    Id = 3,
                    EventId = 1,
                    CategoryId = 3,
                    SortOrder = 3,
                    TabLabel = "2×2 MIX",
                    Title = "Torneo di Fine Estate MIX",
                    Format = "2×2",
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(18, 30, 0),
                    Price = 25,
                    MaxTeams = 16,
                    Level = "Open",
                    MinPlayers = 2,
                    FormUrl = "https://docs.google.com/forms/d/e/1FAIpQLScNsRCRIIgpqGM1kE3xl3ZjnHVu6-BF0fViXYKSO8scBSaGcQ/viewform?usp=header",
                    WaitlistFormUrl = ""
                }
            );

            modelBuilder.Entity<Pool>()
                    .HasOne(p => p.Tournament)
                    .WithMany(t => t.Pools)
                    .HasForeignKey(p => p.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoolEntry>()
                .HasOne(pe => pe.Pool)
                .WithMany(p => p.Entries)
                .HasForeignKey(pe => pe.PoolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoolEntry>()
                .HasOne(pe => pe.Registration)
                .WithMany()
                .HasForeignKey(pe => pe.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Court>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Courts)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourtAvailability>()
                .HasOne(ca => ca.Court)
                .WithMany(c => c.Availabilities)
                .HasForeignKey(ca => ca.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Pool)
                .WithMany()
                .HasForeignKey(m => m.PoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Court)
                .WithMany(c => c.Matches)
                .HasForeignKey(m => m.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team1)
                .WithMany()
                .HasForeignKey(m => m.Team1RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team2)
                .WithMany()
                .HasForeignKey(m => m.Team2RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TournamentCourt>()
                .HasOne(tc => tc.Tournament)
                .WithMany(t => t.TournamentCourts)
                .HasForeignKey(tc => tc.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TournamentCourt>()
                .HasOne(tc => tc.Court)
                .WithMany(c => c.TournamentCourts)
                .HasForeignKey(tc => tc.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TournamentCourt>()
                .HasIndex(tc => new
                {
                    tc.TournamentId,
                    tc.CourtId
                })
                .IsUnique();

            modelBuilder.Entity<FinalPhase>()
                .HasOne(fp => fp.Tournament)
                .WithMany()
                .HasForeignKey(fp => fp.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinalPhaseQualification>()
                .HasOne(fq => fq.FinalPhase)
                .WithMany(fp => fp.Qualifications)
                .HasForeignKey(fq => fq.FinalPhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinalPhaseQualification>()
                .HasIndex(fq => fq.FinalPhaseId);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.FinalPhase)
                .WithMany()
                .HasForeignKey(fm => fm.FinalPhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.Court)
                .WithMany()
                .HasForeignKey(fm => fm.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.Team1)
                .WithMany()
                .HasForeignKey(fm => fm.Team1RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.Team2)
                .WithMany()
                .HasForeignKey(fm => fm.Team2RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.Team1SourceMatch)
                .WithMany()
                .HasForeignKey(fm => fm.Team1SourceMatchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinalMatch>()
                .HasOne(fm => fm.Team2SourceMatch)
                .WithMany()
                .HasForeignKey(fm => fm.Team2SourceMatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}