using Cinema_BusinessLayer;
using Cinema_BusinessLayer.Interfaces;
using Cinema_BusinessLayer.Models;
using Cinema_DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace Cinema_DataAccessLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IBaseRepository<Customer> Customers { get; private set; }
        public IBaseRepository<Hall> Halls { get; private set; }
        public IBaseRepository<Movie> Movies { get; private set; }
        public IBaseRepository<Seat> Seats { get; private set; }
        public IBaseRepository<Showtime> Showtimes { get; private set; }
        public IBaseRepository<Ticket> Tickets { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Customers = new BaseRepository<Customer>(_context);
            Halls = new BaseRepository<Hall>(_context);
            Movies = new BaseRepository<Movie>(_context);
            Seats = new BaseRepository<Seat>(_context);
            Showtimes = new BaseRepository<Showtime>(_context);
            Tickets = new BaseRepository<Ticket>(_context);
        }

        public EntityEntry Entry(object entity)
        {
            return _context.Entry(entity);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
