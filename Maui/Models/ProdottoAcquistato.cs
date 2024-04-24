using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maui.Models
{
    public class ProdottoAcquistato
    {
        [Key]
        public int IdProdottoAcquistato { get; set; }

        [Required]
        [ForeignKey("Ordine")]
        public int IdOrdine { get; set; }

        [Required]
        [ForeignKey("Prodotto")]
        public int IdProdotto { get; set; }

        [Required]
        public int Quantita { get; set; }
        [NotMapped]
        [ForeignKey("Prodotto")]
        public string NomeProdotto { get; set; }

        public virtual Prodotto Prodotto { get; set; }
        public virtual Ordine Ordine { get; set; }
    }
}
