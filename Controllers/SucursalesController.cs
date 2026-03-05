using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppEnvios.Data;
using WebAppEnvios.Models;

namespace WebAppEnvios.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SucursalesController : Controller
    {
        private readonly AppDbContext _context;

        public SucursalesController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Sucursales.OrderBy(s => s.Departamento).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales
                .Include(s => s.Envios)
                .FirstOrDefaultAsync(m => m.SucursalId == id);
            return sucursal == null ? NotFound() : View(sucursal);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SucursalId,Nombre,Departamento,Direccion,Telefono,Activa")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales.FindAsync(id);
            return sucursal == null ? NotFound() : View(sucursal);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SucursalId,Nombre,Departamento,Direccion,Telefono,Activa")] Sucursal sucursal)
        {
            if (id != sucursal.SucursalId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(sucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales.FirstOrDefaultAsync(m => m.SucursalId == id);
            return sucursal == null ? NotFound() : View(sucursal);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal != null) _context.Sucursales.Remove(sucursal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
