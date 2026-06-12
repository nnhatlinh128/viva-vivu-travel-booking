using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Repositories.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Booking> BookingRepository { get; }
    IGenericRepository<Tour> TourRepository { get; }
    IGenericRepository<Destination> DestinationRepository { get; }
    IGenericRepository<TourItinerary> TourItineraryRepository { get; }
    IGenericRepository<FavoriteTour> FavoriteTourRepository { get; }
    IGenericRepository<Voucher> VoucherRepository { get; }
    IGenericRepository<MembershipTier> MembershipTierRepository { get; }
    Task CompleteAsync();
}
