using Microsoft.AspNetCore.Mvc;
using StockApi.Data;
using StockApi.Models;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCategorias()
        {
            var categorias = _context.Categorias.ToList();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategoria(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria == null) return NotFound();
            return Ok(categoria);
        }

        [HttpPost]
        public IActionResult CrearCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public IActionResult ActualizarCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id) return BadRequest();

            var existente = _context.Categorias.Find(id);
            if (existente == null) return NotFound();

            existente.Nombre = categoria.Nombre;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarCategoria(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria == null) return NotFound();

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
            return NoContent();
        }
    }
}