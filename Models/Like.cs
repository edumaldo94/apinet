using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apinet.Models
{
    public class Like
    {
        [Key]
        public int LikeID { get; set; }
        
        [ForeignKey("Usuario")]
        public int? UsuarioID { get; set; }
        [ForeignKey("Receta")]
        public int? RecetaID { get; set; } 
    }
}
