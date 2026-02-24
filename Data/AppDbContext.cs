using Microsoft.EntityFrameworkCore;
using WebAppEnvios.Models;
using WebAppEnvios.Models.WebAppEnvios.Models;

namespace WebAppEnvios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Destinatario> Destinatarios { get; set; }
        public DbSet<EstadoEnvio> EstadosEnvio { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Paquete> Paquetes { get; set; }

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

            modelBuilder.Entity<Paquete>()
                .HasOne(p => p.Envio)
                .WithMany(e => e.Paquetes)
                .HasForeignKey(p => p.EnvioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
