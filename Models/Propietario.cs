using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace apinet.Models
{
    public class Propietario
    {
         [Key]
        public int id_Propietario { get; set; }

        [DisplayName("Nombre")]
        public string? Nombre { get; set; }

        [DisplayName("Apellido")]
        public string? Apellido { get; set; }

        [DisplayName("Nombre")]
        public string? Dni { get; set; }

        [DisplayName("Email")]
        public string? Email { get; set; }
        
        public string? Clave { get; set; }

        [DisplayName("Telefono")]
        public string? Telefono { get; set; }

        [DisplayName("Estado")]
        public int? EstadoP { get; set; }
            public string? Avatar {get; set; }
     /*          [NotMapped]
    public string ClaveAntigua { get; set; }

    [NotMapped]
    public string ClaveNueva { get; set; }

    [NotMapped]
    public string ConfirmarClaveNueva { get; set; }*/

        public override string ToString()
		{
			//return $"{Apellido}, {Nombre}";
			//return $"{Nombre} {Apellido}";
			var res = $"{Nombre} {Apellido}";
			if(!String.IsNullOrEmpty(Dni)) {
				res += $" ({Dni})";
			}
			return res;
		}
    }
}

