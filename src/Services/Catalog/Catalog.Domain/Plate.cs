namespace Catalog.Domain
{
    public class Plate
    {
        public Guid Id { get; set; }

        public string? Registration { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SalePrice { get; set; }
        public decimal SalePriceWithMarkup => SalePrice * 1.2m;

        public string? Letters { get; set; }

        public int Numbers { get; set; }
    }
}