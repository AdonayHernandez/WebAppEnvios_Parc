using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppEnvios.Data;
using WebAppEnvios.Models;

namespace WebAppEnvios.Controllers
{
    [Authorize]
    public class EnviosController : Controller
    {
        private readonly AppDbContext _context;
        // Estados que permiten edición/eliminación por el cliente
        private static readonly string[] EstadosEditables = { "Pendiente" };

        public EnviosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Envios
        public async Task<IActionResult> Index()
        {
            var enviosQuery = _context.Envios
                .Include(e => e.Cliente)
                .Include(e => e.Destinatario)
                .Include(e => e.EstadoEnvio)
                .Include(e => e.Sucursal);

            if (User.IsInRole("Administrador"))
            {
                return View(await enviosQuery.OrderByDescending(e => e.FechaEnvio).ToListAsync());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cliente == null)
                return View(new List<Envio>());

            return View(await enviosQuery
                .Where(e => e.ClienteId == cliente.ClienteId)
                .OrderByDescending(e => e.FechaEnvio)
                .ToListAsync());
        }

        // GET: Envios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var envio = await _context.Envios
                .Include(e => e.Cliente)
                .Include(e => e.Destinatario)
                .Include(e => e.EstadoEnvio)
                .Include(e => e.Sucursal)
                .Include(e => e.Paquetes)
                .Include(e => e.Pagos)
                .FirstOrDefaultAsync(m => m.EnvioId == id);

            if (envio == null) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();
            }

            return View(envio);
        }

        // GET: Envios/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Administrador"))
            {
                ViewData["DestinatarioId"] = new SelectList(_context.Destinatarios.OrderBy(d => d.Nombre), "DestinatarioId", "Nombre");
                ViewData["ClienteId"]      = new SelectList(_context.Clientes.OrderBy(c => c.Nombre), "ClienteId", "Nombre");
            }
            else
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente != null)
                {
                    ViewData["DestinatarioId"] = new SelectList(
                        _context.Destinatarios.Where(d => d.ClienteId == cliente.ClienteId).OrderBy(d => d.Nombre),
                        "DestinatarioId", "Nombre");
                }
                else
                {
                    ViewData["DestinatarioId"] = new SelectList(new List<Destinatario>(), "DestinatarioId", "Nombre");
                }
            }

            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa).OrderBy(s => s.Departamento), "SucursalId", "Nombre");
            return View();
        }

        // POST: Envios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnvioId,DestinatarioId,SucursalId,FechaEnvio,FechaEntrega,Costo,Observaciones")] Envio envio)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clienteInfo = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);

            if (clienteInfo == null)
            {
                clienteInfo = new Cliente
                {
                    UserId    = userId,
                    Nombre    = User.Identity?.Name ?? "Usuario Anónimo",
                    Email     = User.Identity?.Name ?? "sin_correo",
                    Direccion = "No registrada",
                    Telefono  = "No registrado"
                };
                _context.Clientes.Add(clienteInfo);
                await _context.SaveChangesAsync();
            }

            envio.ClienteId = clienteInfo.ClienteId;

            // Estado inicial siempre Pendiente
            var estadoPendiente = await _context.EstadosEnvio.FirstOrDefaultAsync(e => e.NombreEstado == "Pendiente");
            if (estadoPendiente == null)
            {
                estadoPendiente = new EstadoEnvio { NombreEstado = "Pendiente", Descripcion = "Envío registrado." };
                _context.EstadosEnvio.Add(estadoPendiente);
                await _context.SaveChangesAsync();
            }

            envio.EstadoId = estadoPendiente.EstadoId;

            // Generar número de seguimiento único
            envio.NumeroSeguimiento = $"SB-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";

            ModelState.Remove("Cliente");
            ModelState.Remove("EstadoEnvio");
            ModelState.Remove("Destinatario");
            ModelState.Remove("Paquetes");
            ModelState.Remove("Pagos");
            ModelState.Remove("Sucursal");
            ModelState.Remove("NumeroSeguimiento");

            if (ModelState.IsValid)
            {
                _context.Add(envio);
                await _context.SaveChangesAsync();

                // Registrar historial: Estado inicial Pendiente
                _context.HistorialEstados.Add(new HistorialEstado
                {
                    EnvioId        = envio.EnvioId,
                    EstadoAnterior = null,
                    EstadoNuevo    = "Pendiente",
                    FechaCambio    = DateTime.Now,
                    CambiadoPor    = User.Identity?.Name,
                    Observaciones  = "Envío creado por el cliente."
                });

                // Crear pago pendiente automáticamente
                var pago = new Pago
                {
                    EnvioId    = envio.EnvioId,
                    Monto      = envio.Costo,
                    Estado     = EstadoPago.Pendiente,
                    Metodo     = MetodoPago.Efectivo,
                    Comision   = 5 // 5% comisión por defecto
                };
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = envio.EnvioId });
            }

            if (User.IsInRole("Administrador"))
            {
                ViewData["DestinatarioId"] = new SelectList(_context.Destinatarios, "DestinatarioId", "Nombre", envio.DestinatarioId);
                ViewData["ClienteId"]      = new SelectList(_context.Clientes, "ClienteId", "Nombre", envio.ClienteId);
            }
            else
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                ViewData["DestinatarioId"] = new SelectList(
                    _context.Destinatarios.Where(d => d.ClienteId == clienteInfo.ClienteId),
                    "DestinatarioId", "Nombre", envio.DestinatarioId);
            }

            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa).OrderBy(s => s.Departamento), "SucursalId", "Nombre", envio.SucursalId);
            return View(envio);
        }

        // GET: Envios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var envio = await _context.Envios
                .Include(e => e.EstadoEnvio)
                .FirstOrDefaultAsync(e => e.EnvioId == id);

            if (envio == null) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();

                // Bloquear edición si no está Pendiente
                if (!EstadosEditables.Contains(envio.EstadoEnvio?.NombreEstado))
                {
                    TempData["Error"] = "Solo puedes editar envíos que estén en estado Pendiente (antes de recolección).";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["DestinatarioId"] = new SelectList(
                    _context.Destinatarios.Where(d => d.ClienteId == cliente.ClienteId).OrderBy(d => d.Nombre),
                    "DestinatarioId", "Nombre", envio.DestinatarioId);
            }
            else
            {
                ViewData["ClienteId"]      = new SelectList(_context.Clientes, "ClienteId", "Nombre", envio.ClienteId);
                ViewData["DestinatarioId"] = new SelectList(_context.Destinatarios, "DestinatarioId", "Nombre", envio.DestinatarioId);
                ViewData["EstadoId"]       = new SelectList(_context.EstadosEnvio, "EstadoId", "NombreEstado", envio.EstadoId);
            }

            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa).OrderBy(s => s.Departamento), "SucursalId", "Nombre", envio.SucursalId);
            return View(envio);
        }

        // POST: Envios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnvioId,ClienteId,DestinatarioId,EstadoId,SucursalId,FechaEnvio,FechaEntrega,Costo,Observaciones,NumeroSeguimiento")] Envio envio)
        {
            if (id != envio.EnvioId) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();

                // Doble verificación del estado desde la BD
                var envioDb = await _context.Envios.Include(e => e.EstadoEnvio).AsNoTracking().FirstOrDefaultAsync(e => e.EnvioId == id);
                if (!EstadosEditables.Contains(envioDb?.EstadoEnvio?.NombreEstado))
                {
                    TempData["Error"] = "No puedes editar un envío que ya fue recolectado o está en tránsito.";
                    return RedirectToAction(nameof(Index));
                }

                // El cliente no puede cambiar el estado
                envio.EstadoId = envioDb!.EstadoId;
            }

            ModelState.Remove("Cliente");
            ModelState.Remove("EstadoEnvio");
            ModelState.Remove("Destinatario");
            ModelState.Remove("Paquetes");
            ModelState.Remove("Pagos");
            ModelState.Remove("Sucursal");

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el estado cambió para registrar historial
                    var envioAnterior = await _context.Envios
                        .Include(e => e.EstadoEnvio)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EnvioId == envio.EnvioId);

                    _context.Update(envio);
                    await _context.SaveChangesAsync();

                    if (envioAnterior != null && envioAnterior.EstadoId != envio.EstadoId)
                    {
                        var estadoNuevo = await _context.EstadosEnvio.FindAsync(envio.EstadoId);
                        _context.HistorialEstados.Add(new HistorialEstado
                        {
                            EnvioId        = envio.EnvioId,
                            EstadoAnterior = envioAnterior.EstadoEnvio?.NombreEstado,
                            EstadoNuevo    = estadoNuevo?.NombreEstado ?? "Desconocido",
                            FechaCambio    = DateTime.Now,
                            CambiadoPor    = User.Identity?.Name,
                            Observaciones  = envio.Observaciones
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnvioExists(envio.EnvioId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = envio.EnvioId });
            }

            ViewData["DestinatarioId"] = new SelectList(_context.Destinatarios, "DestinatarioId", "Nombre", envio.DestinatarioId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa), "SucursalId", "Nombre", envio.SucursalId);
            return View(envio);
        }

        // GET: Envios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var envio = await _context.Envios
                .Include(e => e.Cliente)
                .Include(e => e.Destinatario)
                .Include(e => e.EstadoEnvio)
                .Include(e => e.Sucursal)
                .FirstOrDefaultAsync(m => m.EnvioId == id);

            if (envio == null) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();

                if (!EstadosEditables.Contains(envio.EstadoEnvio?.NombreEstado))
                {
                    TempData["Error"] = "Solo puedes cancelar envíos en estado Pendiente (antes de recolección).";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(envio);
        }

        // POST: Envios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var envio = await _context.Envios
                .Include(e => e.EstadoEnvio)
                .FirstOrDefaultAsync(e => e.EnvioId == id);

            if (envio != null)
            {
                if (!User.IsInRole("Administrador"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cliente == null || envio.ClienteId != cliente.ClienteId)
                        return Forbid();

                    if (!EstadosEditables.Contains(envio.EstadoEnvio?.NombreEstado))
                    {
                        TempData["Error"] = "No puedes cancelar un envío que ya está en proceso.";
                        return RedirectToAction(nameof(Index));
                    }

                    // El cliente "cancela" el envío en vez de eliminarlo
                    var estadoCancelado = await _context.EstadosEnvio.FirstOrDefaultAsync(e => e.NombreEstado == "Cancelado");
                    if (estadoCancelado != null)
                    {
                        var estadoAnteriorNombre = envio.EstadoEnvio?.NombreEstado;
                        envio.EstadoId = estadoCancelado.EstadoId;
                        _context.Update(envio);

                        // Registrar en historial
                        _context.HistorialEstados.Add(new HistorialEstado
                        {
                            EnvioId        = envio.EnvioId,
                            EstadoAnterior = estadoAnteriorNombre,
                            EstadoNuevo    = "Cancelado",
                            FechaCambio    = DateTime.Now,
                            CambiadoPor    = User.Identity?.Name,
                            Observaciones  = "Cancelado por el cliente antes de recolección."
                        });

                        await _context.SaveChangesAsync();
                        TempData["Success"] = "El envío fue cancelado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.Envios.Remove(envio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Envios/Etiqueta/5
        public async Task<IActionResult> Etiqueta(int? id)
        {
            if (id == null) return NotFound();

            var envio = await _context.Envios
                .Include(e => e.Cliente)
                .Include(e => e.Destinatario)
                .Include(e => e.EstadoEnvio)
                .Include(e => e.Sucursal)
                .Include(e => e.Paquetes)
                .FirstOrDefaultAsync(m => m.EnvioId == id);

            if (envio == null) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();
            }

            return View(envio);
        }

        // GET: Envios/Historial/5
        public async Task<IActionResult> Historial(int? id)
        {
            if (id == null) return NotFound();

            var envio = await _context.Envios
                .Include(e => e.Cliente)
                .FirstOrDefaultAsync(e => e.EnvioId == id);

            if (envio == null) return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || envio.ClienteId != cliente.ClienteId)
                    return Forbid();
            }

            var historial = await _context.HistorialEstados
                .Where(h => h.EnvioId == id)
                .OrderByDescending(h => h.FechaCambio)
                .ToListAsync();

            ViewBag.EnvioId            = id;
            ViewBag.NumeroSeguimiento  = envio.NumeroSeguimiento ?? $"SB-{id}";

            return View(historial);
        }

        private bool EnvioExists(int id) => _context.Envios.Any(e => e.EnvioId == id);
    }
}
