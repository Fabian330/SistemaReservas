using Microsoft.EntityFrameworkCore;
using SistemaReservas.Models;

namespace SistemaReservas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        }
        public DbSet<SistemaReservas.Models.Usuario> Usuario { get; set; } = default!;
        public DbSet<SistemaReservas.Models.Rol> Rol { get; set; } = default!;
        public DbSet<SistemaReservas.Models.Reservas> Reservas { get; set; } = default!;
    }
}
