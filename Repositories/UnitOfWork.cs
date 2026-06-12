using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IGenericRepository<Booking> _bookingRepository;
    private IGenericRepository<Tour> _tourRepository;
    private IGenericRepository<Destination> _destinationRepository;
    private IGenericRepository<TourItinerary> _tourItineraryRepository;
    private IGenericRepository<FavoriteTour> _favoriteTourRepository;
    private IGenericRepository<Voucher> _voucherRepository;
    private IGenericRepository<MembershipTier> _membershipTierRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Booking> BookingRepository
    {
        get
        {
            if (_bookingRepository == null)
            {
                _bookingRepository = new GenericRepository<Booking>(_context);
            }
            return _bookingRepository;
        }
    }

    public IGenericRepository<Tour> TourRepository
    {
        get
        {
            if (_tourRepository == null)
            {
                _tourRepository = new GenericRepository<Tour>(_context);
            }
            return _tourRepository;
        }
    }

    public IGenericRepository<Destination> DestinationRepository
    {
        get
        {
            if (_destinationRepository == null)
            {
                _destinationRepository = new GenericRepository<Destination>(_context);
            }
            return _destinationRepository;
        }
    }

    public IGenericRepository<TourItinerary> TourItineraryRepository
    {
        get
        {
            if (_tourItineraryRepository == null)
            {
                _tourItineraryRepository = new GenericRepository<TourItinerary>(_context);
            }
            return _tourItineraryRepository;
        }
    }

    public IGenericRepository<FavoriteTour> FavoriteTourRepository
    {
        get
        {
            if (_favoriteTourRepository == null)
            {
                _favoriteTourRepository = new GenericRepository<FavoriteTour>(_context);
            }
            return _favoriteTourRepository;
        }
    }

    public IGenericRepository<Voucher> VoucherRepository
    {
        get
        {
            if (_voucherRepository == null)
                _voucherRepository = new GenericRepository<Voucher>(_context);
            return _voucherRepository;
        }
    }
    public IGenericRepository<MembershipTier> MembershipTierRepository
{
    get
    {
        if (_membershipTierRepository == null)
        {
            _membershipTierRepository = new GenericRepository<MembershipTier>(_context);
        }
        return _membershipTierRepository;
    }
}


    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}