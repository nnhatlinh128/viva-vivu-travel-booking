using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<TourItinerary> TourItineraries { get; set; }
    public DbSet<FavoriteTour> FavoriteTours { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}