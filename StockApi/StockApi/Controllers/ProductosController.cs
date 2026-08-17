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


        // GET: api/productos/exportar
        [HttpGet("exportar")]
        public IActionResult ExportarExcel()
        {
            var productos = _context.Productos.ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var hoja = workbook.Worksheets.Add("Productos");

            // Encabezados
            hoja.Cell(1, 1).Value = "Id";
            hoja.Cell(1, 2).Value = "Nombre";
            hoja.Cell(1, 3).Value = "Descripcion";
            hoja.Cell(1, 4).Value = "Precio";
            hoja.Cell(1, 5).Value = "StockActual";
            hoja.Cell(1, 6).Value = "StockMinimo";
            hoja.Cell(1, 7).Value = "CategoriaId";
            hoja.Row(1).Style.Font.Bold = true;

            // Filas de datos
            int fila = 2;
            foreach (var p in productos)
            {
                hoja.Cell(fila, 1).Value = p.Id;
                hoja.Cell(fila, 2).Value = p.Nombre;
                hoja.Cell(fila, 3).Value = p.Descripcion;
                hoja.Cell(fila, 4).Value = p.Precio;
                hoja.Cell(fila, 5).Value = p.StockActual;
                hoja.Cell(fila, 6).Value = p.StockMinimo;
                hoja.Cell(fila, 7).Value = p.CategoriaId;
                fila++;
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var contenido = stream.ToArray();

            return File(
                contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "productos.xlsx"
            );
        }
    

    // POST: api/productos/importar
[HttpPost("importar")]
        public async Task<IActionResult> ImportarExcel(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            var productosCreados = new List<Producto>();
            var errores = new List<string>();

            using var stream = new MemoryStream();
            await archivo.CopyToAsync(stream);

            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var hoja = workbook.Worksheet(1);

            var filas = hoja.RowsUsed().Skip(1); // Saltea la fila de encabezados

            foreach (var fila in filas)
            {
                try
                {
                    var nombre = fila.Cell(2).GetString();
                    var descripcion = fila.Cell(3).GetString();
                    var precio = fila.Cell(4).GetValue<decimal>();
                    var stockActual = fila.Cell(5).GetValue<int>();
                    var stockMinimo = fila.Cell(6).GetValue<int>();
                    var categoriaId = fila.Cell(7).GetValue<int>();

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        errores.Add($"Fila {fila.RowNumber()}: el nombre está vacío, se omitió.");
                        continue;
                    }

                    var categoriaExiste = _context.Categorias.Any(c => c.Id == categoriaId);
                    if (!categoriaExiste)
                    {
                        errores.Add($"Fila {fila.RowNumber()}: la categoría {categoriaId} no existe, se omitió.");
                        continue;
                    }

                    var producto = new Producto
                    {
                        Nombre = nombre,
                        Descripcion = descripcion,
                        Precio = precio,
                        StockActual = stockActual,
                        StockMinimo = stockMinimo,
                        CategoriaId = categoriaId
                    };

                    _context.Productos.Add(producto);
                    productosCreados.Add(producto);
                }
                catch (Exception ex)
                {
                    errores.Add($"Fila {fila.RowNumber()}: error al procesar ({ex.Message}).");
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                creados = productosCreados.Count,
                errores
            });
        }
    }
}


public class MovimientoDto
    {
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
    }
