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
        }
    }
}