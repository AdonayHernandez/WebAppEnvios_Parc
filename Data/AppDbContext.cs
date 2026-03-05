using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAppEnvios.Models;

namespace WebAppEnvios.Data
{
    public class AppDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Destinatario> Destinatarios { get; set; }
        public DbSet<EstadoEnvio> EstadosEnvio { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Paquete> Paquetes { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique(false);

            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Cliente)
                .WithMany(c => c.Envios)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Destinatario)
                .WithMany(d => d.Envios)
                .HasForeignKey(e => e.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Envio>()
                .HasOne(e => e.EstadoEnvio)
                .WithMany(es => es.Envios)
                .HasForeignKey(e => e.EstadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Sucursal)
                .WithMany(s => s.Envios)
                .HasForeignKey(e => e.SucursalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Paquete>()
                .HasOne(p => p.Envio)
                .WithMany(e => e.Paquetes)
                .HasForeignKey(p => p.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Envio)
                .WithMany(e => e.Pagos)
                .HasForeignKey(p => p.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistorialEstado>()
                .HasOne(h => h.Envio)
                .WithMany(e => e.HistorialEstados)
                .HasForeignKey(h => h.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed: Estados base del sistema
            modelBuilder.Entity<EstadoEnvio>().HasData(
                new EstadoEnvio { EstadoId = 1, NombreEstado = "Pendiente",      Descripcion = "Orden registrada, en espera de recolección." },
                new EstadoEnvio { EstadoId = 2, NombreEstado = "Recolectado",    Descripcion = "Paquete recogido por mensajero." },
                new EstadoEnvio { EstadoId = 3, NombreEstado = "En Tránsito",    Descripcion = "Paquete en camino a destino." },
                new EstadoEnvio { EstadoId = 4, NombreEstado = "En Sucursal",    Descripcion = "Paquete disponible en sucursal destino." },
                new EstadoEnvio { EstadoId = 5, NombreEstado = "Entregado",      Descripcion = "Paquete entregado al destinatario." },
                new EstadoEnvio { EstadoId = 6, NombreEstado = "Cancelado",      Descripcion = "Envío cancelado por el cliente." },
                new EstadoEnvio { EstadoId = 7, NombreEstado = "No Entregado",   Descripcion = "Intento de entrega fallido." }
            );

            // Seed: Sucursales departamentales de El Salvador
            modelBuilder.Entity<Sucursal>().HasData(
                new Sucursal { SucursalId = 1,  Nombre = "SmallBox San Salvador Central", Departamento = "San Salvador",      Direccion = "Centro Histórico, San Salvador",       Telefono = "2222-0001", Activa = true },
                new Sucursal { SucursalId = 2,  Nombre = "SmallBox Santa Ana",            Departamento = "Santa Ana",          Direccion = "4a Calle Ote, Santa Ana",              Telefono = "2222-0002", Activa = true },
                new Sucursal { SucursalId = 3,  Nombre = "SmallBox San Miguel",           Departamento = "San Miguel",         Direccion = "Av. Roosevelt, San Miguel",            Telefono = "2222-0003", Activa = true },
                new Sucursal { SucursalId = 4,  Nombre = "SmallBox La Libertad",          Departamento = "La Libertad",        Direccion = "Calle Melchor Velásquez, Nueva San Salvador", Telefono = "2222-0004", Activa = true },
                new Sucursal { SucursalId = 5,  Nombre = "SmallBox Sonsonate",            Departamento = "Sonsonate",          Direccion = "Av. Morán, Sonsonate",                 Telefono = "2222-0005", Activa = true },
                new Sucursal { SucursalId = 6,  Nombre = "SmallBox Usulután",             Departamento = "Usulután",           Direccion = "1a Av. Sur, Usulután",                 Telefono = "2222-0006", Activa = true },
                new Sucursal { SucursalId = 7,  Nombre = "SmallBox Chalatenango",         Departamento = "Chalatenango",       Direccion = "Calle Principal, Chalatenango",        Telefono = "2222-0007", Activa = true },
                new Sucursal { SucursalId = 8,  Nombre = "SmallBox Cuscatlán",            Departamento = "Cuscatlán",          Direccion = "Coja, Suchitoto",                      Telefono = "2222-0008", Activa = true },
                new Sucursal { SucursalId = 9,  Nombre = "SmallBox La Paz",               Departamento = "La Paz",             Direccion = "Centro, Zacatecoluca",                 Telefono = "2222-0009", Activa = true },
                new Sucursal { SucursalId = 10, Nombre = "SmallBox Cabañas",              Departamento = "Cabañas",            Direccion = "Sensuntepeque Centro",                 Telefono = "2222-0010", Activa = true },
                new Sucursal { SucursalId = 11, Nombre = "SmallBox San Vicente",          Departamento = "San Vicente",        Direccion = "Parque Central, San Vicente",          Telefono = "2222-0011", Activa = true },
                new Sucursal { SucursalId = 12, Nombre = "SmallBox Ahuachapán",           Departamento = "Ahuachapán",         Direccion = "2a Av. Norte, Ahuachapán",             Telefono = "2222-0012", Activa = true },
                new Sucursal { SucursalId = 13, Nombre = "SmallBox Morazán",              Departamento = "Morazán",            Direccion = "San Francisco Gotera Centro",          Telefono = "2222-0013", Activa = true },
                new Sucursal { SucursalId = 14, Nombre = "SmallBox La Unión",             Departamento = "La Unión",           Direccion = "Puerto, La Unión",                     Telefono = "2222-0014", Activa = true }
            );
        }

        // ─── Auditoría automática ─────────────────────────────────────────────
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var usuario = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";
            var ahora   = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.FechaCreacion = ahora;
                    entry.Entity.CreadoPor     = usuario;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.FechaModificacion = ahora;
                    entry.Entity.ModificadoPor     = usuario;
                    // No sobreescribir FechaCreacion ni CreadoPor en modificaciones
                    entry.Property(e => e.FechaCreacion).IsModified = false;
                    entry.Property(e => e.CreadoPor).IsModified      = false;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
