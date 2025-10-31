using Cinema_BusinessLayer.Interfaces;
using Cinema_BusinessLayer.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<Customer> Customers { get; }
        IBaseRepository<Hall> Halls { get; }
        IBaseRepository<Movie> Movies { get; }
        IBaseRepository<Seat> Seats { get; }
        IBaseRepository<Showtime> Showtimes { get; }
        IBaseRepository<Ticket> Tickets { get; }

        int Complete();
        EntityEntry Entry(object entity);

    }
}
