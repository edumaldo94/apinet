using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apinet.Models
{
       public class RecetaFavorita
    {
        public int RecetaId { get; set; }
        public int UsuarioId { get; set; }

        // Otras propiedades
    }
}