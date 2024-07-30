using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace apinet.Models;

    public class Inquilino
    {
          [Key] 
        public int id_Inquilino { get; set; }
   
        
         [DisplayName("Nombre")]
        public string? Nombre { get; set; }


        [DisplayName("Apellido")]
        public string? Apellido { get; set; }


        [DisplayName("Documento")]
        public string? Dni { get; set; }



       [DisplayName("Email")]
        public string? Email { get; set; }


       [DisplayName("Telefono")]
        public string? Telefono { get; set; }
        
       [DisplayName("Estado")]
        public string? Estado { get; set; }
        
     
    }
