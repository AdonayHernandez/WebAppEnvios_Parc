using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppEnvios.Data;
using WebAppEnvios.Models;

namespace WebAppEnvios.Controllers
{
    [Authorize]
    public class MetricasController : Controller
    {
        private readonly AppDbContext _context;

        public MetricasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Administrador"))
            {
                // Panel de métricas global para administrador
                ViewBag.TotalEnvios      = await _context.Envios.CountAsync();
                ViewBag.EnviosPendientes = await _context.Envios.CountAsync(e => e.EstadoEnvio.NombreEstado == "Pendiente");
                ViewBag.EnviosEnTransito = await _context.Envios.CountAsync(e => e.EstadoEnvio.NombreEstado == "En Tránsito");
                ViewBag.EnviosEntregados = await _context.Envios.CountAsync(e => e.EstadoEnvio.NombreEstado == "Entregado");
                ViewBag.EnviosCancelados = await _context.Envios.CountAsync(e => e.EstadoEnvio.NombreEstado == "Cancelado");
                ViewBag.TotalClientes    = await _context.Clientes.CountAsync();
                ViewBag.TotalSucursales  = await _context.Sucursales.CountAsync(s => s.Activa);
                ViewBag.IngresoTotal     = await _context.Pagos.Where(p => p.Estado == EstadoPago.Pagado).SumAsync(p => (decimal?)p.Monto) ?? 0;
                ViewBag.PagosPendientes  = await _context.Pagos.CountAsync(p => p.Estado == EstadoPago.Pendiente);
                ViewBag.ComisionTotal    = await _context.Pagos.Where(p => p.Estado == EstadoPago.Pagado).SumAsync(p => (decimal?)(p.Monto * p.Comision / 100)) ?? 0;

                // Top sucursales
                ViewBag.TopSucursales = await _context.Envios
                    .Where(e => e.SucursalId != null)
                    .GroupBy(e => e.Sucursal.Nombre)
                    .Select(g => new { Sucursal = g.Key, Total = g.Count() })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .ToListAsync();

                // Por estado
                ViewBag.PorEstado = await _context.Envios
                    .GroupBy(e => e.EstadoEnvio.NombreEstado)
                    .Select(g => new { Estado = g.Key, Total = g.Count() })
                    .OrderByDescending(x => x.Total)
                    .ToListAsync();

                return View("IndexAdmin");
            }
            else
            {
                // Panel de métricas personales del cliente
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null)
                {
                    ViewBag.ClienteId = 0;
                    return View("IndexCliente");
                }

                ViewBag.ClienteId        = cliente.ClienteId;
                ViewBag.NombreCliente    = cliente.Nombre;
                ViewBag.TotalEnvios      = await _context.Envios.CountAsync(e => e.ClienteId == cliente.ClienteId);
                ViewBag.EnviosPendientes = await _context.Envios.CountAsync(e => e.ClienteId == cliente.ClienteId && e.EstadoEnvio.NombreEstado == "Pendiente");
                ViewBag.EnviosEnTransito = await _context.Envios.CountAsync(e => e.ClienteId == cliente.ClienteId && e.EstadoEnvio.NombreEstado == "En Tránsito");
                ViewBag.EnviosEntregados = await _context.Envios.CountAsync(e => e.ClienteId == cliente.ClienteId && e.EstadoEnvio.NombreEstado == "Entregado");
                ViewBag.EnviosCancelados = await _context.Envios.CountAsync(e => e.ClienteId == cliente.ClienteId && e.EstadoEnvio.NombreEstado == "Cancelado");
                ViewBag.TotalDestinatarios = await _context.Destinatarios.CountAsync(d => d.ClienteId == cliente.ClienteId);
                ViewBag.GastoTotal       = await _context.Pagos
                    .Where(p => p.Envio.ClienteId == cliente.ClienteId && p.Estado == EstadoPago.Pagado)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0;
                ViewBag.PagosPendientes  = await _context.Pagos
                    .CountAsync(p => p.Envio.ClienteId == cliente.ClienteId && p.Estado == EstadoPago.Pendiente);

                // Últimos 5 envíos
                ViewBag.UltimosEnvios = await _context.Envios
                    .Include(e => e.Destinatario)
                    .Include(e => e.EstadoEnvio)
                    .Where(e => e.ClienteId == cliente.ClienteId)
                    .OrderByDescending(e => e.FechaEnvio)
                    .Take(5)
                    .ToListAsync();

                return View("IndexCliente");
            }
        }
    }
}
