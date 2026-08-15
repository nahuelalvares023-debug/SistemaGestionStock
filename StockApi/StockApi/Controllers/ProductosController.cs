using Microsoft.AspNetCore.Mvc;
using StockApi.Data;
using StockApi.Models;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productos
        [HttpGet]
        public IActionResult GetProductos()
        {
            var productos = _context.Productos.ToList();
            return Ok(productos);
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public IActionResult GetProducto(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();
            return Ok(producto);
        }

        // POST: api/productos
        [HttpPost]
        public IActionResult CrearProducto(Producto producto)
        {
            _context.Productos.Add(producto);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        // PUT: api/productos/5
        [HttpPut("{id}")]
        public IActionResult ActualizarProducto(int id, Producto producto)
        {
            if (id != producto.Id) return BadRequest();

            var existente = _context.Productos.Find(id);
            if (existente == null) return NotFound();

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.StockMinimo = producto.StockMinimo;
            existente.CategoriaId = producto.CategoriaId;
            // Ojo: StockActual NO se toca acá, solo se modifica via movimientos

            _context.SaveChanges();
            return NoContent();
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        public IActionResult EliminarProducto(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();

            _context.Productos.Remove(producto);
            _context.SaveChanges();
            return NoContent();
        }

        // GET: api/productos/stock-bajo
        [HttpGet("stock-bajo")]
        public IActionResult GetStockBajo()
        {
            var productos = _context.Productos
                .Where(p => p.StockActual < p.StockMinimo)
                .ToList();

            return Ok(productos);
        }

        // POST: api/productos/5/entrada
        [HttpPost("{id}/entrada")]
        public IActionResult RegistrarEntrada(int id, MovimientoDto dto)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound("Producto no encontrado.");

            if (dto.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");

            producto.StockActual += dto.Cantidad;

            var movimiento = new MovimientoStock
            {
                ProductoId = id,
                Tipo = TipoMovimiento.Entrada,
                Cantidad = dto.Cantidad,
                Motivo = dto.Motivo
            };

            _context.Movimientos.Add(movimiento);
            _context.SaveChanges();

            return Ok(new { mensaje = "Entrada registrada.", stockActual = producto.StockActual });
        }

        // POST: api/productos/5/salida
        [HttpPost("{id}/salida")]
        public IActionResult RegistrarSalida(int id, MovimientoDto dto)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound("Producto no encontrado.");

            if (dto.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");

            if (producto.StockActual < dto.Cantidad)
                return BadRequest($"Stock insuficiente. Stock actual: {producto.StockActual}.");

            producto.StockActual -= dto.Cantidad;

            var movimiento = new MovimientoStock
            {
                ProductoId = id,
                Tipo = TipoMovimiento.Salida,
                Cantidad = dto.Cantidad,
                Motivo = dto.Motivo
            };

            _context.Movimientos.Add(movimiento);
            _context.SaveChanges();

            return Ok(new { mensaje = "Salida registrada.", stockActual = producto.StockActual });
        }

        // GET: api/productos/5/movimientos
        [HttpGet("{id}/movimientos")]
        public IActionResult GetMovimientos(int id)
        {
            var movimientos = _context.Movimientos
                .Where(m => m.ProductoId == id)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            return Ok(movimientos);
        }
    }

    public class MovimientoDto
    {
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
    }
}