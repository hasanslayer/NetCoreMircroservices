namespace Shopping.Aggregator.Models
{
    public class BasketModel
    {
        public string userName { get; set; }
        public List<BasketItemExtendedModel> Items { get; set; } = new List<BasketItemExtendedModel>();
        public decimal TotalPrice { get; set; }
    }
}
