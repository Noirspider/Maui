
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maui.Class;
using System.ComponentModel.DataAnnotations;


namespace Maui.Models
{
    public class Pagamento
    {
        [Key]
        public int IdCarta { get; set; }

        [Required]
        public string NumeroCarta { get; set; }

        [Required]
        public string Intestatario { get; set; }

        [Required]
        public DateTime DataScadenza { get; set; }

        [Required]
        public string CVV { get; set; }

        // Chiave esterna
        public int UtenteId { get; set; }

        // Proprietà di navigazione
        public virtual Utente Utente { get; set; }
    }
}

