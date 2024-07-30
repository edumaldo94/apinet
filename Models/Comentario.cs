
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace apinet.Models
{
    public class Comentario
    {
        [Key]
        public int ComentarioID { get; set; }
        
        [ForeignKey("Usuario")]
        public int? UsuarioID { get; set; }
        public Usuario Usuario { get; set; }
        
        [ForeignKey("Receta")]
        public int? RecetaID { get; set; }
        public Receta Receta { get; set; }
        
         [Required]
        public string Coment { get; set; }
        
        public DateTime FechaHora { get; set; } = DateTime.Now;
        
        public bool Eliminado { get; set; } = false;
    }
}
