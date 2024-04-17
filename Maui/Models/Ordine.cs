using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Maui.Models
{
    public class Ordine
    {
        [Key]
        public int IdOrdine { get; set; }

        [Required]
        [ForeignKey("Utente")]
        public int IdUtente { get; set; }

        [Required]
        public string IndirizzoDiConsegna { get; set; }

        [Required]
        public DateTime DataOrdine { get; set; }

        [Required]
        public bool IsEvaso { get; set; } = false;
        public string Nota { get; set; }

        [NotMapped]
        public decimal PrezzoTotale
        {
            get
            {
                if (ProdottoAcquistato == null)
                {
                    return 0;
                }

                decimal prezzoTotale = 0;
                foreach (var prodotto in ProdottoAcquistato)
                {
                    prezzoTotale += prodotto.Prodotto.PrezzoProdotto;
                }
                return prezzoTotale;
            }
            set { }
        }

        public virtual Utente Utente { get; set; }
        public virtual ICollection<ProdottoAcquistato> ProdottoAcquistato { get; set; }
    }
}
