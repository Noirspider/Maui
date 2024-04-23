using Maui.Class;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maui.Models
{
    public class Utente
    {
        [Key]
        public int IdUtente { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = UserRole.USER;

        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        public virtual ICollection<Ordine> Ordine { get; set; }
        //[NotMapped]
        //public virtual ICollection<Pagamento> Pagamento { get; set; }
    }
}