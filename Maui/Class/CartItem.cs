namespace Maui.Models
{
    public class CartItem
    {
        public int IdProdotto { get; set; }
        public string ImgProdotto { get; set; }
        public string NomeProdotto { get; set; }
        public decimal PrezzoProdotto { get; set; }
        public string Stile { get; set; }

        public string Birrificio { get; set; }
        public int Quantita { get; set; }
    }

}
