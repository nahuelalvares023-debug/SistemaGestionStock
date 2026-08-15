namespace StockApi.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}