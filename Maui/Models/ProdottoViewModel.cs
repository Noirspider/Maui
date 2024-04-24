   using Microsoft.AspNetCore.Http;
   using System.ComponentModel.DataAnnotations;

   namespace Maui.Models
   {
       public class ProdottoViewModel : Prodotto
       {
           [Required]
           public IFormFile ImgProdotto { get; set; }
       }
   }
   