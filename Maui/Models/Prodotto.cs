using System.ComponentModel.DataAnnotations;

namespace Maui.Models
{
    public class Prodotto
    {
        [Key]
        public int IdProdotto { get; set; }

        [Required]
        public string NomeProdotto { get; set; }

        [Required]
        public string ImgProdotto { get; set; }

        [Required]
        public decimal PrezzoProdotto { get; set; }
        [Required]
        public int QuantitaProdotto { get; set; }
        public string Stile { get; set; }

        public string Descrizione { get; set; }

        public string Volume { get; set; }
        public string Gradazione { get; set; }
        public string Birrificio { get; set; }
        public string Nazione { get; set; }

        public virtual ICollection<ProdottoAcquistato> ProdottiAcquistati { get; set; }

    }
}
