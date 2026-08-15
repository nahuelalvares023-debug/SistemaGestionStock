namespace StockApi.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
    }
}