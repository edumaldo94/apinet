using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace apinet.Models
{
    public class Receta
    {
        [Key]
        public int RecetaID { get; set; }
        
    
        public int? UsuarioID { get; set; }
        public Usuario Usuario { get; set; }
        
        
        public string? Titulo { get; set; }
        
        public string? Descripcion { get; set; }
        
        public string? Ingredientes { get; set; }
        
        public string? Pasos { get; set; }
        
        public string? Porciones { get; set; }
        
        public string? TiempoPreparacion { get; set; }
        
        public string? Dificultad { get; set; }
        
        public string? TipoCocina { get; set; }
        
        public string? FotoPortada { get; set; }
   public DateTime? FechaPublicacion { get; set; }
        public bool IsDeleted { get; set; } = false; 
        public List<Comentario>? Comentarios { get; set; }
        
        public List<Like>? Like { get; set; }
   
    public List<RecetaFavorita>? RecetasFavoritas { get; set; }
          public Receta()
{
    Like = new List<Like>();
    Comentarios = new List<Comentario>();
    RecetasFavoritas = new List<RecetaFavorita>();
}
   
    }
    
}
